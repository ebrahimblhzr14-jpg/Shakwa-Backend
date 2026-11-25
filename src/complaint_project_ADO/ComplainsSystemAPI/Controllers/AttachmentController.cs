using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using ComplaintManagement.Business.DTOs.Attachments;
using ComplaintManagement.Business.DTOs.Complaints;
using ComplaintManagement.Business.Interfaces;
using ComplaintManagement.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Security.Claims;

namespace ComplainsSystemAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AttachmentController : ControllerBase
    {
        private readonly IAttachmentService _attachmentService;
        private readonly IComplaintService _complaintService;
        private readonly IAssignmentsRepository _assignmentsRepository;

        public AttachmentController(
            IAttachmentService attachmentService,
            IComplaintService complaintService,
            IAssignmentsRepository assignmentsRepository)
        {
            _attachmentService = attachmentService;
            _complaintService = complaintService;
            _assignmentsRepository = assignmentsRepository;
        }

        // =====================================================
        // POST: api/Attachment/Upload/{complainId} (Canonical)
        // POST: api/Attachment/Uploade/{complainId} (Legacy Alias)
        // =====================================================

        [HttpPost("Upload/{complainId:int}")]
        [HttpPost("Uploade/{complainId:int}")]
        public async Task<IActionResult> SaveImageAsync(int complainId, IFormFile imageFile)
        {
            if (complainId <= 0)
            {
                return BadRequest(new { message = "Invalid complaint ID." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded or file is empty." });
            }

            // 5 MB maximum file size enforcement (5,242,880 bytes)
            const long maxFileSize = 5 * 1024 * 1024;
            if (imageFile.Length > maxFileSize)
            {
                return BadRequest(new { message = "File size exceeds 5 MB limit." });
            }

            // Allowed extension whitelist (images only)
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Invalid file type. Only JPG, JPEG, and PNG images are allowed." });
            }

            // Allowed MIME type whitelist
            var contentType = imageFile.ContentType.ToLowerInvariant();
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/jpg" };
            if (!allowedMimeTypes.Contains(contentType))
            {
                return BadRequest(new { message = "Invalid MIME content type." });
            }

            // Magic-byte signature inspection & cross-check agreement
            if (!ValidateImageSignature(imageFile, extension, contentType))
            {
                return BadRequest(new { message = "File content signature does not match the declared image format." });
            }

            // Verify complaint existence before creating files
            var comp = await _complaintService.GetByIdAsync(complainId);
            if (comp == null)
            {
                return NotFound(new { message = "Complaint not found." });
            }

            // Verify actor authorization (Owner, actively assigned Admin, or SuperAdmin)
            if (!await CanAccessComplaintAttachmentsAsync(comp.UserId, complainId))
            {
                return Forbid();
            }

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Secure server-generated GUID filename (never trust client filename for filesystem storage)
            var safeName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(folderPath, safeName);

            var dto = new CreateAttachmentDto
            {
                ComplaintId = complainId,
                FileName = safeName,
                FilePath = filePath,
                FileType = contentType,
                FileSize = imageFile.Length
            };

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
                await _attachmentService.CreateAsync(GetCurrentUserId(), dto);
            }

            return Ok(new
            {
                FileName = dto.FileName,
                FileSizeBytes = dto.FileSize,
                ContentType = dto.FileType
            });
        }

        // =====================================================
        // GET: api/Attachment/GetImagePath/{complainID}
        // =====================================================

        [HttpGet("GetImagePath/{complainID:int}")]
        public async Task<IActionResult> GetImagePath(int complainID)
        {
            if (complainID <= 0)
            {
                return BadRequest(new { message = "Invalid complaint ID." });
            }

            var comp = await _complaintService.GetByIdAsync(complainID);
            if (comp == null)
            {
                return NotFound(new { message = "Complaint not found." });
            }

            if (!await CanAccessComplaintAttachmentsAsync(comp.UserId, complainID))
            {
                return Forbid();
            }

            var attachmentList = await _attachmentService.GetByComplaintAsync(complainID);
            if (attachmentList == null || attachmentList.Count == 0)
            {
                return NotFound(new { message = "No attachments found for this complaint." });
            }

            string fileName = attachmentList[0].FileName;
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", fileName);

            if (!System.IO.File.Exists(path))
            {
                return NotFound(new { message = "Image file not found on disk." });
            }

            string imageUrl = $"{Request.Scheme}://{Request.Host}/Images/{fileName}";
            return Ok(imageUrl);
        }

        private async Task<bool> CanAccessComplaintAttachmentsAsync(int complaintOwnerId, int complaintId)
        {
            if (User.IsInRole("SuperAdmin"))
                return true;

            int currentUserId = GetCurrentUserId();
            if (currentUserId <= 0)
                return false;

            if (User.IsInRole("User"))
            {
                return complaintOwnerId == currentUserId;
            }

            if (User.IsInRole("Admin"))
            {
                return await _assignmentsRepository.HasActiveAssignmentAsync(complaintId, currentUserId);
            }

            return false;
        }

        private static bool ValidateImageSignature(IFormFile file, string extension, string contentType)
        {
            try
            {
                using var stream = file.OpenReadStream();
                if (stream.Length < 8)
                    return false;

                byte[] header = new byte[8];
                int bytesRead = stream.Read(header, 0, 8);
                if (bytesRead < 8)
                    return false;

                // PNG signature: 89 50 4E 47 0D 0A 1A 0A
                if (extension == ".png" && contentType == "image/png")
                {
                    return header[0] == 0x89 &&
                           header[1] == 0x50 &&
                           header[2] == 0x4E &&
                           header[3] == 0x47 &&
                           header[4] == 0x0D &&
                           header[5] == 0x0A &&
                           header[6] == 0x1A &&
                           header[7] == 0x0A;
                }

                // JPEG signature: FF D8 FF
                if ((extension == ".jpg" || extension == ".jpeg") &&
                    (contentType == "image/jpeg" || contentType == "image/jpg"))
                {
                    return header[0] == 0xFF &&
                           header[1] == 0xD8 &&
                           header[2] == 0xFF;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private int GetCurrentUserId()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userId, out int id) ? id : 0;
        }

        // =====================================================
        // PUT: api/Attachment/Update/{id} (Preserved as-is)
        // =====================================================

        [HttpPut("Update/{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(
            int id,
            IFormFile imageFile,
            IAuthorizationService authorizationService)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid attachment ID."
                });
            }

            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest(new
                {
                    message = "Please select a valid image."
                });
            }

            // Get current attachment
            var attachment = await _attachmentService.GetByIdAsync(id);

            if (attachment == null)
            {
                return NotFound(new
                {
                    message = "Attachment not found."
                });
            }

            // Get related complaint
            var complaint = await _complaintService.GetByIdAsync(
                attachment.ComplaintId
            );

            if (complaint == null)
            {
                return NotFound(new
                {
                    message = "Complaint not found."
                });
            }

            // Check authorization
            var authResult = await authorizationService.AuthorizeAsync(
                User,
                complaint.UserId,
                "OwnerOrSuperAdmin"
            );

            if (!authResult.Succeeded)
                return Forbid();

            // Get old file path
            var oldFilePath = attachment.FilePath;

            // Generate new file name
            var extension = Path.GetExtension(imageFile.FileName);
            var finalName = Guid.NewGuid().ToString() + extension;

            var folderPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "Images"
            );

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var newFilePath = Path.Combine(folderPath, finalName);

            try
            {
                // Save new file
                using (var stream = new FileStream(
                    newFilePath,
                    FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Prepare update DTO
                var dto = new UpdateAttachmentDto
                {
                    FileName = finalName,
                    FilePath = newFilePath,
                    FileType = imageFile.ContentType,
                    FileSize = imageFile.Length
                };

                // Update database
                bool result = await _attachmentService.UpdateAsync(id, dto);

                if (!result)
                {
                    // Remove new file if database update fails
                    if (System.IO.File.Exists(newFilePath))
                    {
                        System.IO.File.Delete(newFilePath);
                    }

                    return NotFound(new
                    {
                        message = "Attachment not found."
                    });
                }
                // Delete old physical file
                if (System.IO.File.Exists(oldFilePath) && result)
                {
                    System.IO.File.Delete(oldFilePath);
                }

                var updatedAttachment = await _attachmentService.GetByIdAsync(id);

                return Ok(updatedAttachment);
            }
            catch (Exception ex)
            {
                // Remove newly uploaded file if an error occurs
                if (System.IO.File.Exists(newFilePath))
                {
                    System.IO.File.Delete(newFilePath);
                }

                return StatusCode(500, new
                {
                    message = "An error occurred while updating the attachment.",
                    error = ex.Message
                });
            }
        }

        // =====================================================
        // DELETE: api/Attachment/Delete/{id} (Preserved as-is)
        // =====================================================

        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id, IAuthorizationService authorizationService)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid attachment ID."
                });
            }
            int currentUserId = GetCurrentUserId();
            AttachmentDto comm = await _attachmentService.GetByIdAsync(id);
            if (comm == null)
            {
                return NotFound(new { message = "Attachment not found." });
            }

            ComplaintDto dto = await _complaintService.GetByIdAsync(comm.ComplaintId);
            if (dto == null)
            {
                return NotFound(new { message = "Complaint not found." });
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
            var authResult = await authorizationService.AuthorizeAsync(
                User,
                dto.UserId,
                "OwnerOrSuperAdmin");

            if (!authResult.Succeeded)
                return Forbid(); // 403

            try
            {
                bool result = await _attachmentService.DeleteAsync(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Attachment not found."
                    });
                }

                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new
                {
                    message = ex.Message
                });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
        }
    }
}
