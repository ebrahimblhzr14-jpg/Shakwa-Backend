using ComplaintManagement.API.Controllers;
using ComplaintManagement.Business.DTOs.ComplaintHistory;
using ComplaintManagement.Business.DTOs.Complaints;
using ComplaintManagement.Business.Interfaces;
using ComplaintManagement.Tests.TestDoubles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Xunit;

namespace ComplaintManagement.Tests
{
    public class AuthorizationAndRoleBoundaryTests
    {
        private readonly FakeComplaintHistoryService _historyService;
        private readonly FakeComplaintService _compService;
        private readonly FakeAssignmentsRepository _assignRepo;
        private readonly ComplaintHistoryController _historyController;

        public AuthorizationAndRoleBoundaryTests()
        {
            _historyService = new FakeComplaintHistoryService();
            _compService = new FakeComplaintService();
            _assignRepo = new FakeAssignmentsRepository();
            _historyController = new ComplaintHistoryController(_historyService, _compService, _assignRepo);
        }

        private void SetCallerUser(string userId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            _historyController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        [Fact]
        public async Task GlobalHistory_SuperAdminCaller_ReturnsOk()
        {
            SetCallerUser("1", "SuperAdmin");
            _historyService.SeedHistory(new ComplaintHistoryDto { Id = 1, ComplaintId = 10, Action = "Created" });

            // Act: complaintId is null -> global audit request
            var result = await _historyController.GetHistory(null);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task GlobalHistory_AdminCaller_ReturnsForbidden()
        {
            SetCallerUser("2", "Admin");

            // Act
            var result = await _historyController.GetHistory(null);

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status403Forbidden, statusResult.StatusCode);
        }

        [Fact]
        public async Task GlobalHistory_UserCaller_ReturnsForbidden()
        {
            SetCallerUser("3", "User");

            // Act
            var result = await _historyController.GetHistory(null);

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status403Forbidden, statusResult.StatusCode);
        }

        [Fact]
        public async Task ComplaintHistory_OwnerUser_ReturnsOk()
        {
            _compService.SeedComplaint(new ComplaintDto { Id = 10, UserId = 42 });
            SetCallerUser("42", "User"); // Owner

            var result = await _historyController.GetHistory(10);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ComplaintHistory_NonOwnerUser_ReturnsForbidden()
        {
            _compService.SeedComplaint(new ComplaintDto { Id = 10, UserId = 42 });
            SetCallerUser("99", "User"); // Not owner

            var result = await _historyController.GetHistory(10);

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status403Forbidden, statusResult.StatusCode);
        }

        [Fact]
        public async Task ComplaintHistory_ActivelyAssignedAdmin_ReturnsOk()
        {
            _compService.SeedComplaint(new ComplaintDto { Id = 10, UserId = 42 });
            _assignRepo.SeedAssignment(10, 77, isActive: true); // Admin 77 assigned
            SetCallerUser("77", "Admin");

            var result = await _historyController.GetHistory(10);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task ComplaintHistory_UnassignedAdmin_ReturnsForbidden()
        {
            _compService.SeedComplaint(new ComplaintDto { Id = 10, UserId = 42 });
            _assignRepo.SeedAssignment(10, 77, isActive: false); // Unassigned (historic only)
            SetCallerUser("77", "Admin");

            var result = await _historyController.GetHistory(10);

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status403Forbidden, statusResult.StatusCode);
        }

        [Fact]
        public async Task ComplaintHistory_SuperAdminCaller_ReturnsOk()
        {
            _compService.SeedComplaint(new ComplaintDto { Id = 10, UserId = 42 });
            SetCallerUser("1", "SuperAdmin");

            var result = await _historyController.GetHistory(10);

            Assert.IsType<OkObjectResult>(result);
        }
    }

    public class FakeComplaintHistoryService : IComplaintHistoryService
    {
        private readonly List<ComplaintHistoryDto> _history = new();

        public void SeedHistory(ComplaintHistoryDto h) => _history.Add(h);

        public Task<IEnumerable<ComplaintHistoryDto>> GetByComplaintIdAsync(int complaintId) =>
            Task.FromResult<IEnumerable<ComplaintHistoryDto>>(_history.Where(h => h.ComplaintId == complaintId).ToList());

        public Task<IEnumerable<ComplaintHistoryDto>> GetAllGlobalAsync() =>
            Task.FromResult<IEnumerable<ComplaintHistoryDto>>(_history.ToList());

        public Task<ComplaintHistoryDto?> GetByIdAsync(int id) =>
            Task.FromResult(_history.FirstOrDefault(h => h.Id == id));

        public Task<int> CreateAsync(int complaintId, int? userId, string action, string? oldValue, string? newValue, string? description)
        {
            var h = new ComplaintHistoryDto { Id = _history.Count + 1, ComplaintId = complaintId, UserId = userId, Action = action, OldValue = oldValue, NewValue = newValue, Description = description };
            _history.Add(h);
            return Task.FromResult(h.Id);
        }
    }
}
