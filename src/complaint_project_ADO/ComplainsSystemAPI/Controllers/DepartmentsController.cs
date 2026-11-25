using ComplaintManagement.Business.DTOs.Departments;
using ComplaintManagement.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ComplainsSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "SuperAdmin")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(
            IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        // =========================================
        // GET ALL DEPARTMENTS
        // =========================================

        [HttpGet("All")]
        [ProducesResponseType(
            typeof(IEnumerable<DepartmentDto>),
            StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var departments =
                await _departmentService.GetAllAsync();

            return Ok(departments);
        }

        // =========================================
        // GET DEPARTMENT BY ID
        // =========================================

        [HttpGet("FindBy/{id:int}")]
        [ProducesResponseType(
            typeof(DepartmentDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new
                {
                    message = "Invalid department ID."
                });

            var department =
                await _departmentService.GetByIdAsync(id);

            if (department == null)
                return NotFound(new
                {
                    message = "Department not found."
                });

            return Ok(department);
        }

        // =========================================
        // CREATE DEPARTMENT
        // =========================================

        [HttpPost("Create")]
        [ProducesResponseType(
            typeof(DepartmentDto),
            StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(
            [FromBody] CreateDepartmentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest(new
                {
                    message = "Department name is required."
                });
            }

            int departmentId =
                await _departmentService.CreateAsync(dto);

            var department =
                await _departmentService.GetByIdAsync(
                    departmentId);

            if (department == null)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        message = "Department was created but could not be retrieved."
                    });
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = departmentId },
                department);
        }

        // =========================================
        // UPDATE DEPARTMENT
        // =========================================

        [HttpPut("Update/{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateDepartmentDto dto)
        {
            if (id <= 0)
                return BadRequest(new
                {
                    message = "Invalid department ID."
                });

            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new
                {
                    message = "Department name is required."
                });

            bool updated =
                await _departmentService.UpdateAsync(id, dto);

            if (!updated)
                return NotFound(new
                {
                    message = "Department not found."
                });

            var department =
                await _departmentService.GetByIdAsync(id);

            return Ok(department);
        }

        // =========================================
        // DELETE DEPARTMENT
        // =========================================

        [HttpDelete("Delete/{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest(new
                {
                    message = "Invalid department ID."
                });

            bool deleted =
                await _departmentService.DeleteAsync(id);

            if (!deleted)
                return NotFound(new
                {
                    message = "Department not found."
                });

            return NoContent();
        }
    }
}
