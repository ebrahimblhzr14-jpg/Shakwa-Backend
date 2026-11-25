using ComplaintManagement.Business.DTOs.Priorities;
using ComplaintManagement.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ComplainsSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PrioritiesController : ControllerBase
    {
        private readonly IPriorityService _priorityService;

        public PrioritiesController(
            IPriorityService priorityService)
        {
            _priorityService = priorityService;
        }


        // =====================================================
        // GET: api/priorities
        // =====================================================

        [HttpGet("All")]
        [AllowAnonymous]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> GetAll()
        {
            var priorities =
                await _priorityService.GetAllAsync();

            return Ok(priorities);
        }


        // =====================================================
        // GET: api/priorities/{id}
        // =====================================================

        [HttpGet("GetBy/{id:int}")]
        [AllowAnonymous]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid priority ID."
                });
            }

            var priority =
                await _priorityService.GetByIdAsync(id);

            if (priority == null)
            {
                return NotFound(new
                {
                    message = "Priority not found."
                });
            }

            return Ok(priority);
        }


        // =====================================================
        // POST: api/priorities
        // =====================================================

        [HttpPost("Create")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [EnableRateLimiting("CreateInApiLimiter")]

        public async Task<IActionResult> Create(
            [FromBody] CreatePriorityDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                int priorityId =
                    await _priorityService.CreateAsync(dto);

                var priority =
                    await _priorityService.GetByIdAsync(
                        priorityId);

                return CreatedAtAction(
                    nameof(GetById),
                    new
                    {
                        id = priorityId
                    },
                    priority);
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
        // PUT: api/priorities/{id}
        // =====================================================

        [HttpPut("Update/{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdatePriorityDto dto)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid priority ID."
                });
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                bool result =
                    await _priorityService.UpdateAsync(
                        id,
                        dto);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Priority not found."
                    });
                }

                var priority =
                    await _priorityService.GetByIdAsync(id);

                return Ok(priority);
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
        // PATCH: api/priorities/{id}/activate
        // =====================================================

        [HttpPatch("{id:int}/activate")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> Activate(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid priority ID."
                });
            }

            try
            {
                bool result =
                    await _priorityService
                        .ActivateAsync(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Priority not found."
                    });
                }

                return NoContent();
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
        // PATCH: api/priorities/{id}/deactivate
        // =====================================================

        [HttpPatch("{id:int}/deactivate")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> Deactivate(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid priority ID."
                });
            }

            try
            {
                bool result =
                    await _priorityService
                        .DeactivateAsync(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Priority not found."
                    });
                }

                return NoContent();
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