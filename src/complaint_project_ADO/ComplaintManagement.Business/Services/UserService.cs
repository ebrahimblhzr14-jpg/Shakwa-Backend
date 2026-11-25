using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using ComplaintManagement.Business.DTOs.Users;
using ComplaintManagement.Business.Interfaces;
using ComplaintManagement.Business.Security;

namespace ComplaintManagement.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUsersRepository _usersRepository;

        private readonly PasswordHasher _passwordHasher;


        public UserService(
            IUsersRepository usersRepository,
            PasswordHasher passwordHasher)
        {
            _usersRepository = usersRepository;

            _passwordHasher = passwordHasher;
        }


        // =====================================================
        // CREATE
        // =====================================================

        public async Task<int> CreateAsync(
            CreateUserDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                throw new ArgumentException(
                    "Name is required.");
            }


            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                throw new ArgumentException(
                    "Email is required.");
            }


            if (string.IsNullOrWhiteSpace(dto.Password))
            {
                throw new ArgumentException(
                    "Password is required.");
            }


            if (dto.RoleId <= 0)
            {
                throw new ArgumentException(
                    "Invalid role.");
            }


            // Check email

            User? existingUser =
                await _usersRepository
                    .GetByEmailAsync(dto.Email);


            if (existingUser != null)
            {
                throw new InvalidOperationException(
                    "Email already exists.");
            }


            // Hash password

            string passwordHash =
                _passwordHasher.Hash(
                    dto.Password);


            // Create User

            User user = new User
            {
                Name =
                    dto.Name.Trim(),

                Email =
                    dto.Email.Trim(),

                PasswordHash =
                    passwordHash,

                Phone =
                    dto.Phone,

                ImageUrl =
                    dto.ImageUrl,

                RoleId =
                    dto.RoleId,

                IsActive =
                    true
            };


            return await _usersRepository
                .CreateAsync(user);
        }


        // =====================================================
        // GET ALL
        // =====================================================

        public async Task<IEnumerable<UserDto>>
            GetAllAsync()
        {
            var users =
                await _usersRepository
                    .GetAllAsync();

            return users.Select(MapToDto);
        }


        // =====================================================
        // GET BY ID
        // =====================================================

        public async Task<UserDto?> GetByIdAsync(
            int id)
        {
            if (id <= 0)
                return null;


            User? user =
                await _usersRepository
                    .GetByIdAsync(id);


            if (user == null)
                return null;


            return MapToDto(user);
        }


        // =====================================================
        // UPDATE
        // =====================================================

        public async Task<bool> UpdateAsync(
            int id,
            UpdateUserDto dto)
        {
            User? user =
                await _usersRepository
                    .GetByIdAsync(id);


            if (user == null)
                return false;


            // Check whether email belongs
            // to another user

            User? userWithEmail =
                await _usersRepository
                    .GetByEmailAsync(
                        dto.Email);


            if (userWithEmail != null &&
                userWithEmail.Id != id)
            {
                throw new InvalidOperationException(
                    "Email already exists.");
            }


            if (string.IsNullOrWhiteSpace(
                dto.Name))
            {
                throw new ArgumentException(
                    "Name is required.");
            }


            if (string.IsNullOrWhiteSpace(
                dto.Email))
            {
                throw new ArgumentException(
                    "Email is required.");
            }


            if (dto.RoleId <= 0)
            {
                throw new ArgumentException(
                    "Invalid role.");
            }


            user.Name =
                dto.Name.Trim();

            user.Email =
                dto.Email.Trim();

            user.Phone =
                dto.Phone;

            user.ImageUrl =
                dto.ImageUrl;

            user.RoleId =
                dto.RoleId;

            user.IsActive =
                dto.IsActive;


            return await _usersRepository
                .UpdateAsync(user);
        }


        // =====================================================
        // DEACTIVATE
        // =====================================================

        public async Task<bool> DeactivateAsync(int id)
        {
            User? user =
                await _usersRepository
                    .GetByIdAsync(id);

            if (user == null)
                return false;

            return await _usersRepository
                .DeactivateAsync(id);
        }


        // =====================================================
        // ACTIVATE
        // =====================================================

        public async Task<bool> ActivateAsync(int id)
        {
            User? user =
                await _usersRepository
                    .GetByIdAsync(id);

            if (user == null)
                return false;

            return await _usersRepository
                .ActivateAsync(id);
        }

        // =====================================================
        // CHANGE MY PASSWORD
        // =====================================================

        public async Task<bool>
            ChangeMyPasswordAsync(
                int userId,
                ChangeMyPasswordDto dto)
        {
            User? user =
                await _usersRepository
                    .GetByIdAsync(userId);



            if (user == null)
                return false;


            if (!user.IsActive)
                return false;


            if (string.IsNullOrWhiteSpace(
                dto.CurrentPassword))
            {
                throw new ArgumentException(
                    "Current password is required.");
            }


            if (string.IsNullOrWhiteSpace(
                dto.NewPassword))
            {
                throw new ArgumentException(
                    "New password is required.");
            }


            if (dto.NewPassword.Length < 8)
            {
                throw new ArgumentException(
                    "New password must contain at least 8 characters.");
            }

            user = await _usersRepository.GetByEmailAsync(user.Email);
            // Verify current password

            bool valid =
                _passwordHasher.Verify(
                    dto.CurrentPassword,
                    user.PasswordHash);


            if (!valid)
                return false;


            // Prevent same password

            if (_passwordHasher.Verify(
                dto.NewPassword,
                user.PasswordHash))
            {
                throw new InvalidOperationException(
                    "New password must be different from the current password.");
            }


            // Hash new password

            string newPasswordHash =
                _passwordHasher.Hash(
                    dto.NewPassword);


            return await _usersRepository
                .UpdatePasswordAsync(
                    userId,
                    newPasswordHash);
        }


        // =====================================================
        // ADMIN CHANGE PASSWORD
        // =====================================================

        public async Task<bool>
            AdminChangePasswordAsync(
                int userId,
                AdminChangePasswordDto dto)
        {
            User? user =
                await _usersRepository
                    .GetByIdAsync(userId);


            if (user == null)
                return false;


            if (string.IsNullOrWhiteSpace(
                dto.NewPassword))
            {
                throw new ArgumentException(
                    "New password is required.");
            }


            if (dto.NewPassword.Length < 8)
            {
                throw new ArgumentException(
                    "New password must contain at least 8 characters.");
            }


            // Hash new password

            string passwordHash =
                _passwordHasher.Hash(
                    dto.NewPassword);


            return await _usersRepository
                .UpdatePasswordAsync(
                    userId,
                    passwordHash);
        }


       

        // =====================================================
        // MAPPING
        // =====================================================

        private static UserDto MapToDto(
            User user)
        {
            return new UserDto
            {
                Id =
                    user.Id,

                Name =
                    user.Name,

                Email =
                    user.Email,

                Phone =
                    user.Phone,

                ImageUrl =
                    user.ImageUrl,

                RoleId =
                    user.RoleId,

                RoleName =
                    user.RoleName,

                IsActive =
                    user.IsActive,

                CreatedAt =
                    user.CreatedAt
            };
        }

        public Task<bool> PutDepartmentAsync(int Userid, int DepID)
        {
            return _usersRepository.PutDepartmentAsync(Userid, DepID);
        }
    }
}