using ComplaintManagement.Access.Models;
using ComplaintManagement.Business.DTOs.Comments;
using ComplaintManagement.Business.DTOs.Complaints;
using ComplaintManagement.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace ComplainsSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ComplaintCommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IComplaintService _complaintService;


        public ComplaintCommentsController(
            ICommentService commentService,IComplaintService complaint)
        {
            _commentService = commentService;
            _complaintService = complaint;
        }


        // =====================================================
        // GET: api/complaints/{complaintId}/comments
        // =====================================================

        [HttpGet("GetAll/{complaintId:int}")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> GetAll(
            int complaintId, IAuthorizationService authorizationService)
        {
            if (complaintId <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid complaint ID."
                });
            }
            var comp = await _complaintService.GetByIdAsync(complaintId);
            var authResult = await authorizationService.AuthorizeAsync(
    User,
    comp.UserId,
    "OwnerOrAdminOrSuper");

            if (!authResult.Succeeded)
                return Forbid(); // 403

            int currentUserId = GetCurrentUserId();

            if (currentUserId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }


            var comments =
                await _commentService.GetByComplaintAsync(complaintId);

            return Ok(comments);
        }


        // =====================================================
        // GET:
        // api/complaints/{complaintId}/comments/{commentId}
        // =====================================================

        [HttpGet("GetByID/{commentId:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> GetById(
            int commentId)
        {
            if (commentId <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "Invalid complaint or comment ID."
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

            string role = GetCurrentUserRole();

            var comment =
                await _commentService.GetByIdAsync(commentId);

            if (comment == null)
            {
                return NotFound(new
                {
                    message = "Comment not found."
                });
            }

            return Ok(comment);
        }


        // =====================================================
        // POST:
        // api/complaints/{complaintId}/comments
        // =====================================================

        [HttpPost("Create")]
        [EnableRateLimiting("CreateInApiLimiter")]

        public async Task<IActionResult> Create(
            [FromBody] CreateCommentDto dto, IAuthorizationService authorizationService)
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

            var comp =  await _complaintService.GetByIdAsync(dto.ComplaintId);

            var authResult = await authorizationService.AuthorizeAsync(
                User,
                comp.UserId,
                "OwnerOrAdminOrSuper");

            if (!authResult.Succeeded)
                return Forbid(); // 403



            try
            {



                int commentId =
                    await _commentService.CreateAsync(currentUserId,dto);

                var comment =
                    await _commentService.GetByIdAsync(commentId);

                return CreatedAtAction(
                    nameof(GetById),
                    new
                    {
                        commentId = commentId
                    },
                    comment);
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
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }


        // =====================================================
        // PUT:
        // api/complaints/{complaintId}/comments/{commentId}
        // =====================================================

        [HttpPut("{commentId:int}")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> Update(
            int commentId,
            [FromBody] UpdateCommentDto dto, IAuthorizationService authorizationService)
        {
            if (commentId <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "Invalid complaint or comment ID."
                });
            }
            int currentUserId = GetCurrentUserId();
            CommentDto comm=await _commentService.GetByIdAsync(commentId);
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }
            var authResult = await authorizationService.AuthorizeAsync(
            User,
            comm.UserId,
            "OwnerOrSuperAdmin");

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
                bool result =
                    await _commentService.UpdateAsync(
                        commentId,
                        currentUserId,
                        dto);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Comment not found."
                    });
                }

                var comment =
                    await _commentService.GetByIdAsync(commentId);

                return Ok(comment);
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
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }


        // =====================================================
        // DELETE:
        // api/complaints/{complaintId}/comments/{commentId}
        // =====================================================

        [HttpDelete("{commentId:int}")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> Delete(
            int complaintId,
            int commentId, IAuthorizationService authorizationService)
        {
            if (complaintId <= 0 ||
                commentId <= 0)
            {
                return BadRequest(new
                {
                    message =
                        "Invalid complaint or comment ID."
                });
            }

            int currentUserId = GetCurrentUserId();
            CommentDto comm = await _commentService.GetByIdAsync(commentId);
            var authResult = await authorizationService.AuthorizeAsync(
           User,
           comm.UserId,
           "OwnerOrSuperAdmin");

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
                bool result =
                    await _commentService.DeleteAsync(commentId,currentUserId);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Comment not found."
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


        // =====================================================
        // CURRENT USER ID
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
        // CURRENT USER ROLE
        // =====================================================

        private string GetCurrentUserRole()
        {
            return User.FindFirstValue(
                       ClaimTypes.Role)
                   ?? string.Empty;
        }
    }
}