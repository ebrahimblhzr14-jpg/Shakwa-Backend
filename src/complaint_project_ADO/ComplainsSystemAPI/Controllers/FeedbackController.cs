using ComplaintManagement.Access.Models;
using ComplaintManagement.Business.DTOs.Feedback;
using ComplaintManagement.Business.Interfaces;
using ComplaintManagement.Business.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace ComplainsSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;
        private readonly IComplaintService _complaintService;
        public FeedbackController(
            IFeedbackService feedbackService, IComplaintService complaintService)
        {
            _feedbackService = feedbackService;
            _complaintService = complaintService;
        }


        // =====================================================
        // GET
        // api/complaints/{complaintId}/feedback
        // =====================================================

        [HttpGet("FindBy/{complaintId:int}")]
        [HttpGet("FindBy{complaintId:int}")]
        [AllowAnonymous]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> GetByComplaint(
            int complaintId, IAuthorizationService authorizationService)
        {
            if (complaintId <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid complaint ID."
                });
            }

            int currentUserId =
                GetCurrentUserId();
            var complaint =
               await _complaintService.GetByIdAsync(complaintId);
            if (complaint == null)
            {
                return BadRequest(new
                {
                    message =
                        "Invalid complaint or feedback ID."
                });
            }
            var authResult = await authorizationService.AuthorizeAsync(
               User,
               complaint.UserId,
               "OwnerOrAdminOrSuper");

            if (!authResult.Succeeded)
                return Forbid(); // 403

            if (currentUserId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            try
            {
                var feedback =
                    await _feedbackService
                        .GetByComplaintAsync(complaintId);

                if (feedback == null)
                {
                    return NotFound(new
                    {
                        message =
                            "Feedback not found."
                    });
                }

                return Ok(feedback);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
        }


        // =====================================================
        // GET
        // api/complaints/{complaintId}/feedback/{feedbackId}
        // =====================================================

       
        [EnableRateLimiting("GeneralApiLimiter")]
        [HttpGet("FindByFeedID/{complaintId:int}/{feedbackId:int}")]
        public async Task<IActionResult> GetById(
    int complaintId,
    int feedbackId,
    IAuthorizationService authorizationService)
        {
            if (complaintId <= 0 ||
                feedbackId <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "Invalid complaint or feedback ID."
                });
            }
            var complaint =
              await _complaintService.GetByIdAsync(complaintId);
            if (complaint == null)
            {
                return BadRequest(new
                {
                    message =
                        "Invalid complaint or feedback ID."
                });
            }
            var authResult = await authorizationService.AuthorizeAsync(
               User,
               complaint.UserId,
               "OwnerOrAdminOrSuper");

            if (!authResult.Succeeded)
                return Forbid(); // 403

            int currentUserId =
                GetCurrentUserId();

            if (currentUserId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            var feedback =
                await _feedbackService
                    .GetByIdAsync(feedbackId);

            if (feedback == null ||
                feedback.ComplaintId != complaintId)
            {
                return NotFound(new
                {
                    message =
                        "Feedback not found."
                });
            }

            return Ok(feedback);
        }


        // =====================================================
        // POST
        // api/complaints/{complaintId}/feedback
        // =====================================================

        [HttpPost("create")]
        [EnableRateLimiting("CreateInApiLimiter")]

        public async Task<IActionResult> Create(
            [FromBody] CreateFeedbackDto dto)
        {
            if (dto.ComplaintId <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid complaint ID."
                });
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(
                    ModelState);
            }

            int currentUserId =
                GetCurrentUserId();

            if (currentUserId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            try
            {
                int feedbackId =
                    await _feedbackService.CreateAsync(
                        currentUserId,
                        dto);

                var feedback =
                    await _feedbackService.GetByIdAsync(
                        feedbackId);

                return CreatedAtAction(
                     nameof(GetById),
                     new
                     {
                         complaintId = dto.ComplaintId,
                         feedbackId = feedbackId
                     },
                     feedback);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
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


        // =====================================================
        // PUT
        // api/complaints/{complaintId}/feedback/{feedbackId}
        // =====================================================

        [HttpPut("Update/{complaintId:int}/{feedbackId:int}")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> Update(
        int complaintId,
        int feedbackId,
        [FromBody] UpdateFeedbackDto dto,
        IAuthorizationService authorizationService)
        {
            if (complaintId <= 0 ||
                feedbackId <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "Invalid complaint or feedback ID."
                });
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(
                    ModelState);
            }

            int currentUserId =
                GetCurrentUserId();

            if (currentUserId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }
            var complain= await _complaintService.GetByIdAsync(complaintId);
            var authResult = await authorizationService.AuthorizeAsync(
     User,
     complain.UserId,
     "OwnerOrSuperAdmin");


            if (!authResult.Succeeded)
                return Forbid(); // 403


            try
            {
                var existingFeedback =
                    await _feedbackService
                        .GetByIdAsync(feedbackId);

                if (existingFeedback == null ||
                    existingFeedback.ComplaintId !=
                    complaintId)
                {
                    return NotFound(new
                    {
                        message =
                            "Feedback not found."
                    });
                }

                bool result =
                    await _feedbackService.UpdateAsync(
                        feedbackId,
                        currentUserId,
                        dto);

                if (!result)
                {
                    return NotFound(new
                    {
                        message =
                            "Feedback not found."
                    });
                }

                var feedback =
                    await _feedbackService.GetByIdAsync(
                        feedbackId);

                return Ok(feedback);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
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


        // =====================================================
        // Current User ID
        // =====================================================

        private int GetCurrentUserId()
        {
            string? userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return 0;
            }

            return int.TryParse(
                userId,
                out int id)
                    ? id
                    : 0;
        }


        // =====================================================
        // Current User Role
        // =====================================================

        private string GetCurrentUserRole()
        {
            return User.FindFirstValue(
                       ClaimTypes.Role)
                   ?? string.Empty;
        }
    }
}