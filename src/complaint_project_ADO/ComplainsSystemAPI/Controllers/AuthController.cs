using ComplaintManagement.Business.DTOs.Auth;
using ComplaintManagement.Business.DTOs.Users;
using ComplaintManagement.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;


namespace ComplainsSystemAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(
            IAuthService authService)
        {
            _authService = authService;
        }


        // =========================================
        // LOGIN
        // =========================================

        [HttpPost("login")]
        [EnableRateLimiting("AuthLimiter")]
        public async Task<IActionResult> Login(
            [FromBody] LoginDto dto)
        {
            var result =
                await _authService.LoginAsync(dto);


            if (result == null)
            {
                return Unauthorized(new
                {
                    message =
                        "Invalid email or password."
                });
            }


            return Ok(result);
        }


        // =========================================
        // Sing in
        // =========================================

        [HttpPost("SingIn")]
        [EnableRateLimiting("SingInLimiter")]

        public async Task<IActionResult> SingIn(
            [FromBody] SingInInfoDTO dto)
        {
            
            try
            {
                CreateUserDto Cdto= new CreateUserDto();
                Cdto.Email = dto.Email;
                Cdto.Password = dto.Password;
                Cdto.Name = dto.Name;
                Cdto.Phone = dto.Phone;
                Cdto.RoleId = 1;

                var result =
                    await _authService.SinginAsync(Cdto);


                if (result == null)
                {
                    return Unauthorized(new
                    {
                        message =
                            "Invalid email or password."
                    });
                }
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    message = ex.Message
                });
            }

           
        }



        // =========================================
        // REFRESH
        // =========================================

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
     [FromBody] RefreshTokenRequestDto dto)
        {
            var result =
                await _authService
                    .RefreshTokenAsync(
                        dto.RefreshToken);


            if (result == null)
            {
                return Unauthorized(new
                {
                    message =
                        "Invalid or expired refresh token."
                });
            }


            return Ok(result);
        }

        // =========================================
        // LOGOUT
        // =========================================

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(
    [FromBody] RefreshTokenRequestDto dto)
        {
            bool result =
                await _authService
                    .LogoutAsync(
                        dto.RefreshToken);


            if (!result)
            {
                return BadRequest(new
                {
                    message =
                        "Invalid or already revoked refresh token."
                });
            }


            return Ok(new
            {
                message =
                    "Logged out successfully."
            });
        }
    }
}