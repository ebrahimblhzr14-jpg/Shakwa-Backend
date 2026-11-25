using ComplaintManagement.Business.DTOs.Categories;
using ComplaintManagement.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ComplainsSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(
            ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }


        // =====================================================
        // GET: api/categories
        // =====================================================

        [HttpGet("All")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> GetAll()
        {
            var categories =
                await _categoryService.GetAllAsync();

            return Ok(categories);
        }


        // =====================================================
        // GET: api/categories/{id}
        // =====================================================

        [HttpGet("FindBy/{id:int}")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid category ID."
                });
            }

            var category =
                await _categoryService.GetByIdAsync(id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            return Ok(category);
        }


        // =====================================================
        // POST: api/categories
        // =====================================================

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [EnableRateLimiting("CreateInApiLimiter")]

        public async Task<IActionResult> Create(
            [FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                int categoryId =
                    await _categoryService.CreateAsync(dto);

                var category =
                    await _categoryService.GetByIdAsync(
                        categoryId);

                return CreatedAtAction(
                    nameof(GetById),
                    new
                    {
                        id = categoryId
                    },
                    category);
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
        // PUT: api/categories/{id}
        // =====================================================

        [HttpPut("Update/{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateCategoryDto dto)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid category ID."
                });
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            try
            {
                bool result =
                    await _categoryService.UpdateAsync(
                        id,
                        dto);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Category not found."
                    });
                }

                var category =
                    await _categoryService.GetByIdAsync(id);

                return Ok(category);
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



        [HttpPatch("activate/{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> Activate(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid category ID."
                });
            }

            try
            {
                bool result =
                    await _categoryService.ActivateAsync(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Category not found."
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

        [HttpPatch("deactivate/{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult> Deactivate(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid category ID."
                });
            }

            try
            {
                bool result =
                    await _categoryService.DeactivateAsync(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Category not found."
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