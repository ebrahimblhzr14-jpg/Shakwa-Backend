using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using ComplaintManagement.Business.DTOs.Auth;
using ComplaintManagement.Business.DTOs.Users;
using ComplaintManagement.Business.Interfaces;
using ComplaintManagement.Business.Security;

namespace ComplaintManagement.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsersRepository _usersRepository;
        private UserService _userserveces;

        private readonly PasswordHasher _passwordHasher;

        private readonly JwtTokenService _jwtTokenService;

        private readonly IRefreshTokensRepository
            _refreshTokensRepository;

        private readonly RefreshTokenGenerator
            _refreshTokenGenerator;


        public AuthService(
            IUsersRepository usersRepository,
            PasswordHasher passwordHasher,
            JwtTokenService jwtTokenService,
            IRefreshTokensRepository refreshTokensRepository,
            RefreshTokenGenerator refreshTokenGenerator)
        {
            _usersRepository = usersRepository;

            _passwordHasher = passwordHasher;

            _jwtTokenService = jwtTokenService;

            _refreshTokensRepository =
                refreshTokensRepository;

            _refreshTokenGenerator =
                refreshTokenGenerator;
            _userserveces=new UserService(_usersRepository,passwordHasher);
        }


        // =========================================
        // LOGIN
        // =========================================

        public async Task<LoginResponseDto?> LoginAsync(
            LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return null;
            }


            User? user =
                await _usersRepository
                    .GetByEmailAsync(dto.Email);
            
            
            if (user == null)
                return null;


            if (!user.IsActive)
                return null;


            bool passwordValid =
                _passwordHasher.Verify(
                    dto.Password,
                    user.PasswordHash);


            if (!passwordValid)
                return null;


            string role =
                user.RoleName ?? string.Empty;


            string accessToken =
                _jwtTokenService.GenerateToken(
                    user.Id,
                    user.Name,
                    user.Email,
                    role);


            string refreshToken =
                _refreshTokenGenerator.Generate();


            RefreshToken refreshTokenEntity =
                new RefreshToken
                {
                    UserId = user.Id,

                    Token = refreshToken,

                    ExpiresAt =
                        DateTime.UtcNow.AddDays(7),

                    CreatedAt =
                        DateTime.UtcNow
                };


            await _refreshTokensRepository
                .CreateAsync(
                    refreshTokenEntity);


            return new LoginResponseDto
            {
                AccessToken = accessToken,

                RefreshToken = refreshToken
            };
        }


        // =========================================
        // Sing in
        // =========================================

        public async Task<LoginResponseDto?> SinginAsync(
            CreateUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return null;
            }
            
            
            int ID =await _userserveces.CreateAsync(dto);

            User? user =
                await _usersRepository
                    .GetByEmailAsync(dto.Email);


            if (user == null)
                return null;


            if (!user.IsActive)
                return null;


            bool passwordValid =
                _passwordHasher.Verify(
                    dto.Password,
                    user.PasswordHash);


            if (!passwordValid)
                return null;


            string role =
                user.RoleName ?? string.Empty;


            string accessToken =
                _jwtTokenService.GenerateToken(
                    user.Id,
                    user.Name,
                    user.Email,
                    role);


            string refreshToken =
                _refreshTokenGenerator.Generate();


            RefreshToken refreshTokenEntity =
                new RefreshToken
                {
                    UserId = user.Id,

                    Token = refreshToken,

                    ExpiresAt =
                        DateTime.UtcNow.AddDays(7),

                    CreatedAt =
                        DateTime.UtcNow
                };


            await _refreshTokensRepository
                .CreateAsync(
                    refreshTokenEntity);


            return new LoginResponseDto
            {
                AccessToken = accessToken,

                RefreshToken = refreshToken
            };
        }

        // =========================================
        // REFRESH TOKEN
        // =========================================

        public async Task<LoginResponseDto?>
            RefreshTokenAsync(
                string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(
                refreshToken))
            {
                return null;
            }


            // Get refresh token from database

            RefreshToken? storedToken =
                await _refreshTokensRepository
                    .GetByTokenAsync(
                        refreshToken);


            if (storedToken == null)
                return null;


            // Check if revoked

            if (storedToken.RevokedAt != null)
                return null;


            // Check expiration

            if (storedToken.ExpiresAt <=
                DateTime.UtcNow)
            {
                return null;
            }


            // Get user

            User? user =
                await _usersRepository
                    .GetByIdAsync(
                        storedToken.UserId);


            if (user == null)
                return null;


            // Check account

            if (!user.IsActive)
                return null;


            string role =
                user.RoleName ?? string.Empty;


            // Generate new Access Token

            string newAccessToken =
                _jwtTokenService.GenerateToken(
                    user.Id,
                    user.Name,
                    user.Email,
                    role);


            // Generate new Refresh Token

            string newRefreshToken =
                _refreshTokenGenerator.Generate();


            RefreshToken newRefreshTokenEntity =
                new RefreshToken
                {
                    UserId = user.Id,

                    Token = newRefreshToken,

                    ExpiresAt =
                        DateTime.UtcNow.AddDays(7),

                    CreatedAt =
                        DateTime.UtcNow
                };


            // Save new Refresh Token

            await _refreshTokensRepository
                .CreateAsync(
                    newRefreshTokenEntity);


            // Revoke old Refresh Token

            await _refreshTokensRepository
                .RevokeAsync(
                    storedToken.Id);


            // Return new tokens

            return new LoginResponseDto
            {
                AccessToken = newAccessToken,

                RefreshToken = newRefreshToken
            };
        }


        // =========================================
        // LOGOUT
        // =========================================

        public async Task<bool> LogoutAsync(
            string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(
                refreshToken))
            {
                return false;
            }


            RefreshToken? storedToken =
                await _refreshTokensRepository
                    .GetByTokenAsync(
                        refreshToken);


            if (storedToken == null)
                return false;


            if (storedToken.RevokedAt != null)
                return false;


            return await _refreshTokensRepository
                .RevokeAsync(
                    storedToken.Id);
        }
    }
}