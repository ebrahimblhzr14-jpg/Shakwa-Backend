using ComplaintManagement.Business.DTOs.Statuses;
using ComplaintManagement.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ComplainsSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StatusesController : ControllerBase
    {
        private readonly IStatusService _statusService;

        public StatusesController(
            IStatusService statusService)
        {
            _statusService = statusService;
        }


        // =====================================================
        // GET: api/statuses
        // =====================================================

        [HttpGet("All")]
        [AllowAnonymous]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> GetAll()
        {
            var statuses =
                await _statusService.GetAllAsync();

            return Ok(statuses);
        }


        // =====================================================
        // GET: api/statuses/{id}
        // =====================================================

        [HttpGet("FindBy/{id:int}")]
        [AllowAnonymous]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid status ID."
                });
            }

            var status =
                await _statusService.GetByIdAsync(id);

            if (status == null)
            {
                return NotFound(new
                {
                    message = "Status not found."
                });
            }

            return Ok(status);
        }


        // =====================================================
        // POST: api/statuses
        // =====================================================

        [HttpPost("create")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [EnableRateLimiting("CreateInApiLimiter")]

        public async Task<IActionResult> Create(
            [FromBody] CreateStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                int statusId =
                    await _statusService.CreateAsync(dto);

                var status =
                    await _statusService.GetByIdAsync(
                        statusId);

                return CreatedAtAction(
                    nameof(GetById),
                    new
                    {
                        id = statusId
                    },
                    status);
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
        // PUT: api/statuses/{id}
        // =====================================================

        [HttpPut("Update/{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateStatusDto dto)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid status ID."
                });
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                bool result =
                    await _statusService.UpdateAsync(
                        id,
                        dto);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Status not found."
                    });
                }

                var status =
                    await _statusService.GetByIdAsync(id);

                return Ok(status);
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
        // PATCH: api/statuses/{id}/activate
        // =====================================================

        [HttpPatch("activate/{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> Activate(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid status ID."
                });
            }

            try
            {
                bool result =
                    await _statusService
                        .ActivateAsync(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Status not found."
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
        // PATCH: api/statuses/{id}/deactivate
        // =====================================================

        [HttpPatch("deactivate/{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> Deactivate(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid status ID."
                });
            }

            try
            {
                bool result =
                    await _statusService
                        .DeactivateAsync(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Status not found."
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