using ComplainsSystemAPI.Controllers;
using ComplaintManagement.Access.Models;
using ComplaintManagement.Business.DTOs.Attachments;
using ComplaintManagement.Business.DTOs.Complaints;
using ComplaintManagement.Business.Interfaces;
using ComplaintManagement.Tests.TestDoubles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Xunit;

namespace ComplaintManagement.Tests
{
    public class AttachmentSecurityAndValidationTests
    {
        private readonly FakeAssignmentsRepository _assignRepo;
        private readonly FakeAttachmentService _attService;
        private readonly FakeComplaintService _compService;
        private readonly AttachmentController _controller;

        public AttachmentSecurityAndValidationTests()
        {
            _assignRepo = new FakeAssignmentsRepository();
            _attService = new FakeAttachmentService();
            _compService = new FakeComplaintService();
            _controller = new AttachmentController(_attService, _compService, _assignRepo);
        }

        private void SetCallerUser(string userId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        [Fact]
        public async Task SaveImage_ValidPng_ReturnsOk()
        {
            // Arrange: PNG header 89 50 4E 47 0D 0A 1A 0A
            byte[] pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00 };
            var file = new FakeFormFile(pngBytes, "test.png", "image/png");

            _compService.SeedComplaint(new ComplaintDto { Id = 1, UserId = 10 });
            SetCallerUser("10", "User"); // Owner

            // Act
            var result = await _controller.SaveImageAsync(1, file);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task SaveImage_ValidJpeg_ReturnsOk()
        {
            // Arrange: JPEG header FF D8 FF
            byte[] jpegBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };
            var file = new FakeFormFile(jpegBytes, "photo.jpg", "image/jpeg");

            _compService.SeedComplaint(new ComplaintDto { Id = 2, UserId = 10 });
            SetCallerUser("10", "User");

            // Act
            var result = await _controller.SaveImageAsync(2, file);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task SaveImage_SpoofedExecutableAsPng_RejectedWithBadRequest()
        {
            // Arrange: PE Header 4D 5A (MZ executable signature) named test.png
            byte[] exeBytes = new byte[] { 0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00 };
            var file = new FakeFormFile(exeBytes, "test.png", "image/png");

            _compService.SeedComplaint(new ComplaintDto { Id = 3, UserId = 10 });
            SetCallerUser("10", "User");

            // Act
            var result = await _controller.SaveImageAsync(3, file);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("signature does not match", badRequest.Value?.ToString());
        }

        [Fact]
        public async Task SaveImage_PdfFile_RejectedUnderImageOnlyPolicy()
        {
            // Arrange: PDF header %PDF (25 50 44 46)
            byte[] pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x35 };
            var file = new FakeFormFile(pdfBytes, "doc.pdf", "application/pdf");

            _compService.SeedComplaint(new ComplaintDto { Id = 4, UserId = 10 });
            SetCallerUser("10", "User");

            // Act
            var result = await _controller.SaveImageAsync(4, file);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Only JPG, JPEG, and PNG images are allowed", badRequest.Value?.ToString());
        }

        [Fact]
        public async Task SaveImage_Exceeds5MbLimit_RejectedWithBadRequest()
        {
            // Arrange: 5 MB + 1 byte
            byte[] oversized = new byte[5 * 1024 * 1024 + 1];
            oversized[0] = 0x89; oversized[1] = 0x50; oversized[2] = 0x4E; oversized[3] = 0x47;
            oversized[4] = 0x0D; oversized[5] = 0x0A; oversized[6] = 0x1A; oversized[7] = 0x0A;

            var file = new FakeFormFile(oversized, "huge.png", "image/png");

            _compService.SeedComplaint(new ComplaintDto { Id = 5, UserId = 10 });
            SetCallerUser("10", "User");

            // Act
            var result = await _controller.SaveImageAsync(5, file);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("5 MB", badRequest.Value?.ToString());
        }

        [Fact]
        public async Task SaveImage_EmptyFile_RejectedWithBadRequest()
        {
            var file = new FakeFormFile(Array.Empty<byte>(), "empty.png", "image/png");
            _compService.SeedComplaint(new ComplaintDto { Id = 6, UserId = 10 });
            SetCallerUser("10", "User");

            var result = await _controller.SaveImageAsync(6, file);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task SaveImage_NonOwnerUser_ReturnsForbid()
        {
            byte[] pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            var file = new FakeFormFile(pngBytes, "test.png", "image/png");

            _compService.SeedComplaint(new ComplaintDto { Id = 7, UserId = 10 }); // Owned by User 10
            SetCallerUser("99", "User"); // Calling as User 99 (Attacker)

            var result = await _controller.SaveImageAsync(7, file);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task SaveImage_UnassignedAdmin_ReturnsForbid()
        {
            byte[] pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            var file = new FakeFormFile(pngBytes, "test.png", "image/png");

            _compService.SeedComplaint(new ComplaintDto { Id = 8, UserId = 10 });
            // Admin 55 is NOT assigned to Complaint 8
            SetCallerUser("55", "Admin");

            var result = await _controller.SaveImageAsync(8, file);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task SaveImage_ActivelyAssignedAdmin_ReturnsOk()
        {
            byte[] pngBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            var file = new FakeFormFile(pngBytes, "test.png", "image/png");

            _compService.SeedComplaint(new ComplaintDto { Id = 9, UserId = 10 });
            _assignRepo.SeedAssignment(9, 55, isActive: true); // Admin 55 is actively assigned
            SetCallerUser("55", "Admin");

            var result = await _controller.SaveImageAsync(9, file);

            Assert.IsType<OkObjectResult>(result);
        }
    }

    public class FakeFormFile : IFormFile
    {
        private readonly byte[] _content;

        public FakeFormFile(byte[] content, string fileName, string contentType)
        {
            _content = content;
            FileName = fileName;
            ContentType = contentType;
            Length = content.Length;
        }

        public string ContentType { get; set; }
        public string ContentDisposition => $"form-data; name=\"imageFile\"; filename=\"{FileName}\"";
        public IHeaderDictionary Headers => new HeaderDictionary();
        public long Length { get; }
        public string Name => "imageFile";
        public string FileName { get; set; }

        public void CopyTo(Stream target) => target.Write(_content, 0, _content.Length);

        public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default) =>
            await target.WriteAsync(_content, 0, _content.Length, cancellationToken);

        public Stream OpenReadStream() => new MemoryStream(_content);
    }

    public class FakeComplaintService : IComplaintService
    {
        private readonly Dictionary<int, ComplaintDto> _complaints = new();

        public void SeedComplaint(ComplaintDto c) => _complaints[c.Id] = c;

        public Task<ComplaintDto?> GetByIdAsync(int id) =>
            Task.FromResult(_complaints.TryGetValue(id, out var c) ? c : null);

        public Task<int> CreateAsync(int userId, CreateComplaintDto dto) => Task.FromResult(1);
        public Task<IEnumerable<ComplaintDto>> GetAllAsync() => Task.FromResult<IEnumerable<ComplaintDto>>(_complaints.Values);
        public Task<IEnumerable<ComplaintDto>> GetByUserAsync(int userId) => Task.FromResult<IEnumerable<ComplaintDto>>(_complaints.Values);
        public Task<IEnumerable<ComplaintDto>> GetAssignedToAdminAsync(int adminId) => Task.FromResult<IEnumerable<ComplaintDto>>(_complaints.Values);
        public Task<bool> UpdateAsync(int id, int userId, CreateComplaintDto dto) => Task.FromResult(true);
        public Task<bool> DeleteAsync(int id, int userId) => Task.FromResult(true);
        public Task<ComplaintManagement.Business.Enums.StatusUpdateResult> UpdateStatusAsync(int complaintId, int statusId, int userId) =>
            Task.FromResult(ComplaintManagement.Business.Enums.StatusUpdateResult.Success);
    }

    public class FakeAttachmentService : IAttachmentService
    {
        private readonly List<AttachmentDto> _attachments = new();

        public void SeedAttachment(AttachmentDto a) => _attachments.Add(a);

        public Task<int> CreateAsync(int userId, CreateAttachmentDto dto)
        {
            var att = new AttachmentDto
            {
                Id = _attachments.Count + 1,
                ComplaintId = dto.ComplaintId,
                FileName = dto.FileName,
                FilePath = dto.FilePath,
                FileType = dto.FileType,
                FileSize = dto.FileSize
            };
            _attachments.Add(att);
            return Task.FromResult(att.Id);
        }

        public Task<List<AttachmentDto>> GetByComplaintAsync(int complaintId) =>
            Task.FromResult(_attachments.Where(a => a.ComplaintId == complaintId).ToList());

        public Task<AttachmentDto?> GetByIdAsync(int id) =>
            Task.FromResult(_attachments.FirstOrDefault(a => a.Id == id));

        public Task<bool> UpdateAsync(int id, UpdateAttachmentDto dto) => Task.FromResult(true);
        public Task<bool> DeleteAsync(int id) => Task.FromResult(true);
    }
}
