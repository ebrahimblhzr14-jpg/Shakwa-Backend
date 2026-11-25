
using ComplaintManagement.Business.DTOs.Users;
using ComplaintManagement.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ComplainsSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IWebHostEnvironment _environment;

        private const long MaxImageSize = 5 * 1024 * 1024; // 5 MB

        private static readonly HashSet<string> AllowedImageExtensions =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

        public UsersController(
            IUserService userService,
            IWebHostEnvironment environment)
        {
            _userService = userService;
            _environment = environment;
        }


        // =====================================================
        // GET: api/users
        // =====================================================

        [HttpGet("All")]
        [Authorize(Roles = "SuperAdmin" )]
        public async Task<IActionResult> GetAll()
        {
            var users =
                await _userService.GetAllAsync();

            return Ok(users);
        }


        // =====================================================
        // GET: api/users/{id}
        // =====================================================

        [HttpGet("FindBy/{id:int}")]
        //[Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            int id, IAuthorizationService authorizationService)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid user ID."
                });
            }

            var user =
                await _userService.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            var authResult = await authorizationService.AuthorizeAsync(
        User,
        user.Id,
        "OwnerOrSuperAdmin");

            if (!authResult.Succeeded)
                return Forbid(); // 403



            return Ok(user);
        }


        // =====================================================
        // POST: api/users
        // =====================================================

        [HttpPost("Create")]
        [Authorize(Roles = "SuperAdmin")]

        //[Authorize(Roles = "SuperAdmin")]
        [AllowAnonymous]
        public async Task<IActionResult> Create(
            [FromForm] CreateUserDto dto,
            IFormFile? image)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            string? savedImagePath = null;

            try
            {
                if (image != null)
                {
                    var validationError = ValidateImage(image);
                    if (validationError != null)
                        return BadRequest(new { message = validationError });

                    savedImagePath = await SaveUserImageAsync(image);
                    dto.ImageUrl = savedImagePath;
                }
                else
                {
                    // Do not accept arbitrary image URLs from the client.
                    dto.ImageUrl = null;
                }

                int userId =
                    await _userService.CreateAsync(dto);

                var user =
                    await _userService.GetByIdAsync(userId);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = userId },
                    user);
            }
            catch (ArgumentException ex)
            {
                DeleteUserImage(savedImagePath);
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                DeleteUserImage(savedImagePath);
                return Conflict(new
                {
                    message = ex.Message
                });
            }
            catch
            {
                DeleteUserImage(savedImagePath);
                throw;
            }
        }


        // =====================================================
        // PUT: api/users/{id}
        // =====================================================

        [HttpPut("Update/{id:int}")]
        //[Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Update(
            int id,
            [FromForm] UpdateUserDto dto,
            IFormFile? image,
            IAuthorizationService authorizationService)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid user ID."
                });
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }


            var authResult = await authorizationService.AuthorizeAsync(
        User,
        id,
        "OwnerOrSuperAdmin");

            if (!authResult.Succeeded)
                return Forbid(); // 403

            var existingUser = await _userService.GetByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound(new { message = "User not found." });
            }

            string? newImagePath = null;
            string? oldImagePath = existingUser.ImageUrl;

            try
            {
                if (image != null)
                {
                    var validationError = ValidateImage(image);
                    if (validationError != null)
                        return BadRequest(new { message = validationError });

                    newImagePath = await SaveUserImageAsync(image);
                    dto.ImageUrl = newImagePath;
                }
                else
                {
                    // Keep the current image when no replacement is uploaded.
                    dto.ImageUrl = existingUser.ImageUrl;
                }

                bool updated =
                    await _userService.UpdateAsync(
                        id,
                        dto);

                if (!updated)
                {
                    DeleteUserImage(newImagePath);
                    return NotFound(new
                    {
                        message = "User not found."
                    });
                }

                if (newImagePath != null &&
                    !string.Equals(oldImagePath, newImagePath, StringComparison.OrdinalIgnoreCase))
                {
                    DeleteUserImage(oldImagePath);
                }

                var user =
                    await _userService.GetByIdAsync(id);

                return Ok(user);
            }
            catch (ArgumentException ex)
            {
                DeleteUserImage(newImagePath);
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                DeleteUserImage(newImagePath);
                return Conflict(new
                {
                    message = ex.Message
                });
            }
            catch
            {
                DeleteUserImage(newImagePath);
                throw;
            }
        }


        // =====================================================
        // PATCH: api/users/{id}/deactivate
        // =====================================================

        [HttpPatch("{id:int}/deactivate")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Deactivate(
            int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid user ID."
                });
            }

            bool result =
                await _userService
                    .DeactivateAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return NoContent();
        }


        // =====================================================
        // PATCH: api/users/{id}/activate
        // =====================================================

        [HttpPatch("{id:int}/activate")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Activate(
            int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid user ID."
                });
            }

            bool result =
                await _userService
                    .ActivateAsync(id);

            if (!result)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return NoContent();
        }



        [HttpPut("PutDept/")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PutDepartment(
           int UserId,int DepID)
        {
            if (UserId <= 0 || DepID <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid user ID or Dept ID."
                });
            }

            bool result =
                await _userService
                    .PutDepartmentAsync(UserId,DepID);

            if (!result)
            {
                return NotFound(new
                {
                    message = "User or Dept not found."
                });
            }

            return NoContent();
        }


        // =====================================================
        // PUT: api/users/me/password
        // =====================================================

        [HttpPut("me/password")]
        [EnableRateLimiting("GeneralApiLimiter")]

        public async Task<IActionResult>
            ChangeMyPassword(
                [FromBody] ChangeMyPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            int userId =
                GetCurrentUserId();

            if (userId <= 0)
            {
                return Unauthorized(new
                {
                    message = "Invalid user identity."
                });
            }

            try
            {
                bool result =
                    await _userService
                        .ChangeMyPasswordAsync(
                            userId,
                            dto);

                if (!result)
                {
                    return BadRequest(new
                    {
                        message =
                            "Current password is incorrect."
                    });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }


        // =====================================================
        // PUT: api/users/{id}/password
        // =====================================================

        [HttpPut("{id:int}/password")]
        [Authorize(Roles = "SuperAdmin")]

        public async Task<IActionResult>
            AdminChangePassword(
                int id,
                [FromBody] AdminChangePasswordDto dto)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid user ID."
                });
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            bool result =
                await _userService
                    .AdminChangePasswordAsync(
                        id,
                        dto);

            if (!result)
            {
                return NotFound(new
                {
                    message = "User not found."
                });
            }

            return NoContent();
        }








        // =====================================================
        // USER IMAGE HELPERS
        // =====================================================

        private static string? ValidateImage(IFormFile image)
        {
            if (image.Length <= 0)
                return "The uploaded image is empty.";

            if (image.Length > MaxImageSize)
                return "Image size must not exceed 5 MB.";

            var extension = Path.GetExtension(image.FileName);
            if (string.IsNullOrWhiteSpace(extension) ||
                !AllowedImageExtensions.Contains(extension))
            {
                return "Only JPG, JPEG, PNG, and WEBP images are allowed.";
            }

            return null;
        }

        private async Task<string> SaveUserImageAsync(IFormFile image)
        {
            string imagesDirectory = Path.Combine(
                _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"),
                "usersImages");

            Directory.CreateDirectory(imagesDirectory);

            string extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            string fileName = $"user_{Guid.NewGuid():N}{extension}";
            string fullPath = Path.Combine(imagesDirectory, fileName);

            await using var stream = new FileStream(
                fullPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None);

            await image.CopyToAsync(stream);

            return $"/usersImages/{fileName}";
        }

        private void DeleteUserImage(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return;

            // Only delete files stored in this API's usersImages folder.
            string fileName = Path.GetFileName(imageUrl);
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            string imagesDirectory = Path.GetFullPath(Path.Combine(
                _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"),
                "usersImages"));

            string fullPath = Path.GetFullPath(Path.Combine(imagesDirectory, fileName));

            if (!fullPath.StartsWith(
                imagesDirectory + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
                return;

            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }


        // =====================================================
        // GET CURRENT USER ID FROM JWT
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


        private string GetCurrentUserEmail()
        {
            string? Email =
                User.FindFirstValue(
                    ClaimTypes.Email);

            if (string.IsNullOrWhiteSpace(Email))
            {
                return null;
            }

            return Email;
        }


    }
}

