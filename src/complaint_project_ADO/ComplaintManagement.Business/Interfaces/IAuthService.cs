using ComplaintManagement.Business.DTOs.Auth;
using ComplaintManagement.Business.DTOs.Users;

namespace ComplaintManagement.Business.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(
            LoginDto dto);

        Task<LoginResponseDto?> SinginAsync(
           CreateUserDto dto);

        Task<LoginResponseDto?> RefreshTokenAsync(
            string refreshToken);

        Task<bool> LogoutAsync(
            string refreshToken);
    }
}