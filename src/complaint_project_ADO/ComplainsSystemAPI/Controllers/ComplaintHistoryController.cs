using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ComplaintHistoryController : ControllerBase
    {
        private readonly IComplaintHistoryService _historyService;
        private readonly IComplaintService _complaintService;
        private readonly IAssignmentsRepository _assignmentsRepository;

        public ComplaintHistoryController(
            IComplaintHistoryService historyService,
            IComplaintService complaintService,
            IAssignmentsRepository assignmentsRepository)
        {
            _historyService = historyService;
            _complaintService = complaintService;
            _assignmentsRepository = assignmentsRepository;
        }

        // =====================================================
        // GET: api/ComplaintHistory?complaintId={id} (Scoped)
        // OR:  api/ComplaintHistory (Global - SuperAdmin only)
        // =====================================================

        [HttpGet]
        [EnableRateLimiting("GeneralApiLimiter")]
        public async Task<IActionResult> GetHistory([FromQuery] int? complaintId)
        {
            // Case 1: Global audit history request (no complaintId provided)
            if (!complaintId.HasValue || complaintId.Value <= 0)
            {
                if (!User.IsInRole("SuperAdmin"))
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new
                    {
                        message = "Only SuperAdmin is authorized to view global audit history."
                    });
                }

                var globalHistory = await _historyService.GetAllGlobalAsync();
                return Ok(globalHistory);
            }

            // Case 2: Complaint-specific history request
            int targetComplaintId = complaintId.Value;
            var complaint = await _complaintService.GetByIdAsync(targetComplaintId);
            if (complaint == null)
            {
                return NotFound(new
                {
                    message = "Complaint not found."
                });
            }

            int currentUserId = GetCurrentUserId();

            if (!await CanAccessComplaintHistoryAsync(complaint.UserId, targetComplaintId, currentUserId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You are not authorized to view this complaint's history."
                });
            }

            try
            {
                var history = await _historyService.GetByComplaintIdAsync(targetComplaintId);
                return Ok(history);
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
        // GET: api/ComplaintHistory/{historyId}?complaintId={complaintId}
        // =====================================================

        [HttpGet("{historyId:int}")]
        [EnableRateLimiting("GeneralApiLimiter")]
        public async Task<IActionResult> GetById(
            [FromQuery] int complaintId,
            int historyId)
        {
            if (complaintId <= 0 || historyId <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid complaint or history ID."
                });
            }

            var complaint = await _complaintService.GetByIdAsync(complaintId);
            if (complaint == null)
            {
                return NotFound(new
                {
                    message = "Complaint not found."
                });
            }

            int currentUserId = GetCurrentUserId();

            if (!await CanAccessComplaintHistoryAsync(complaint.UserId, complaintId, currentUserId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new
                {
                    message = "You are not authorized to view this complaint's history."
                });
            }

            try
            {
                var history = await _historyService.GetByIdAsync(historyId);

                if (history == null || history.ComplaintId != complaintId)
                {
                    return NotFound(new
                    {
                        message = "History record not found."
                    });
                }

                return Ok(history);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        private async Task<bool> CanAccessComplaintHistoryAsync(int complaintOwnerId, int complaintId, int currentUserId)
        {
            if (User.IsInRole("SuperAdmin"))
                return true;

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

        private int GetCurrentUserId()
        {
            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userId, out int id) ? id : 0;
        }
    }
}
