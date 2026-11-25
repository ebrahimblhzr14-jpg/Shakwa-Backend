using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using ComplaintManagement.Business.DTOs.Assignments;
using ComplaintManagement.Business.DTOs.Complaints;
using ComplaintManagement.Business.Enums;
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
    public class ComplaintsController : ControllerBase
    {
        private readonly IComplaintService _complaintService;
        private readonly IAssignmentService _AssignmentcomplaintService;
        private readonly IAssignmentsRepository _assignmentsRepository;
        private readonly ILogger<ComplaintsController> _logger;
        public ComplaintsController(
            IComplaintService complaintService,
            IAssignmentService Assignment,
            IAssignmentsRepository assignmentsRepository,
            ILogger<ComplaintsController> logger)
        {
            _complaintService = complaintService;
            _AssignmentcomplaintService = Assignment;
            _assignmentsRepository = assignmentsRepository;
            _logger = logger;
        }


        // =====================================================
        // GET: api/complaints/All (SuperAdmin only)
        // =====================================================

        [HttpGet("All")]
        [Authorize(Roles = "SuperAdmin")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> GetAll()
        {
            int currentUserId = GetCurrentUserId();

            if (currentUserId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

          

            var complaints =
                await _complaintService.GetAllAsync();

            return Ok(complaints);
        }


        // =====================================================
        // GET: api/complaints/AssignedToMe (Admin & SuperAdmin)
        // =====================================================

        [HttpGet("AssignedToMe")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [EnableRateLimiting("GeneralApiLimiter")]
        public async Task<IActionResult> GetAssignedToMe()
        {
            int currentUserId = GetCurrentUserId();

            if (currentUserId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity in token."
                });
            }

            var complaints =
                await _complaintService.GetAssignedToAdminAsync(currentUserId);

            return Ok(complaints);
        }




        [HttpGet("AllByUserID/{ID:int}")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> GetAllbyUserID(int ID, IAuthorizationService authorizationService)
        {
            int currentUserId = GetCurrentUserId();

            if (currentUserId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            var authResult = await authorizationService.AuthorizeAsync(
       User,
       ID,
       "OwnerOrSuperAdmin");

            if (!authResult.Succeeded)
                return Forbid(); // 403




           

            var complaints =
                await _complaintService.GetByUserAsync(ID);

            return Ok(complaints);
        }

        // =====================================================
        // GET: api/complaints/{id}
        // =====================================================

        [HttpGet("FindBy/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> GetById(int id, IAuthorizationService authorizationService)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid complaint ID."
                });
            }

            int currentUserId = GetCurrentUserId();

            if (currentUserId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            

            var complaint =
                await _complaintService.GetByIdAsync(id);

            if (complaint == null)
            {
                return NotFound(new
                {
                    message = "Complaint not found."
                });
            }
            var authResult = await authorizationService.AuthorizeAsync(
                User,
                complaint.UserId,
                "OwnerOrAdminOrSuper");

            if (!authResult.Succeeded)
            {
                _logger.LogError(
      "User with ID {UserId} Forbiden from Access A complaint that he do not create",
      currentUserId);

                
                return Forbid(); // 403
            }


            return Ok(complaint);
        }


        // =====================================================
        // POST: api/complaints
        // =====================================================

        [HttpPost("Create")]
        [Authorize(Roles = "User,Admin,SuperAdmin")]
        [EnableRateLimiting("CreateInApiLimiter")]


        public async Task<IActionResult> Create(
            [FromBody] CreateComplaintDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            int currentUserId = GetCurrentUserId();

            if (currentUserId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            try
            {
                int complaintId =
                    await _complaintService.CreateAsync(
                        currentUserId,
                        dto);

                var complaint =
                    await _complaintService.GetByIdAsync(complaintId);

                return CreatedAtAction(nameof(GetById),
                    new
                    {
                        id = complaintId
                    },
                    complaint);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }


        // =====================================================
        // PUT: api/complaints/{id}
        // =====================================================

        [HttpPut("Update/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> Update(
            int id,
            [FromBody] CreateComplaintDto dto, IAuthorizationService authorizationService)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid complaint ID."
                });
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            int currentUserId = GetCurrentUserId();

            if (currentUserId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            try
            {

                var authResult = await authorizationService.AuthorizeAsync(
                     User,
                     id,
                     "OwnerOrSuperAdmin");

                if (!authResult.Succeeded)
                    return Forbid(); // 403


                bool result =
                    await _complaintService.UpdateAsync(id,GetCurrentUserId(),dto);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Complaint not found."
                    });
                }

                var complaint =
                    await _complaintService.GetByIdAsync(id);


                return Ok(complaint);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }


        // =====================================================
        // PATCH: api/complaints/{id}/assign
        // =====================================================

        [HttpPatch("assign")]
        [Authorize(Roles = "SuperAdmin")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> Assign(
            [FromBody] CreateAssignmentDto dto)
        {
            if (dto.ComplaintId <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid complaint ID."
                });
            }
            if (dto.AssignedToUserId <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid User ID."
                });
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                int result =
                    await _AssignmentcomplaintService.CreateAsync(GetCurrentUserId(),dto);

                if (result <=0)
                {
                    return BadRequest();
                }
                await _complaintService.UpdateStatusAsync(dto.ComplaintId, 3, GetCurrentUserId());
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }


        // =====================================================
        // PATCH: api/complaints/{id}/status
        // =====================================================

        [HttpPatch("{id:int}/status")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [EnableRateLimiting("GeneralApiLimiter")]
        public async Task<IActionResult> ChangeStatus(
            int id,
            [FromBody] UpdateComplaintStatusDto dto)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid complaint ID."
                });
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            int currentUserId = GetCurrentUserId();
            if (currentUserId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            // Admin authorization: must be actively assigned to this complaint
            if (User.IsInRole("Admin") && !await _assignmentsRepository.HasActiveAssignmentAsync(id, currentUserId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You are not actively assigned to this complaint."
                });
            }

            var result = await _complaintService.UpdateStatusAsync(id, dto.StatusId, currentUserId);

            return result switch
            {
                StatusUpdateResult.Success => NoContent(),
                StatusUpdateResult.NotFound => NotFound(new
                {
                    message = "Complaint not found."
                }),
                StatusUpdateResult.AlreadyClosed => Conflict(new
                {
                    message = "Complaint is already closed."
                }),
                StatusUpdateResult.CannotModifyClosed => BadRequest(new
                {
                    message = "Cannot modify status of a closed complaint."
                }),
                StatusUpdateResult.InvalidTransition => BadRequest(new
                {
                    message = "Invalid status transition."
                }),
                _ => BadRequest(new
                {
                    message = "An error occurred while updating complaint status."
                })
            };
        }


        // =====================================================
        // GET CURRENT USER ID
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
        // GET CURRENT USER ROLE
        // =====================================================

        private string GetCurrentUserRole()
        {
            return User.FindFirstValue(
                       ClaimTypes.Role)
                   ?? string.Empty;
        }
    }
}