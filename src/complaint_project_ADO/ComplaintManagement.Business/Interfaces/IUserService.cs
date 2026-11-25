using ComplaintManagement.Business.DTOs.Users;

namespace ComplaintManagement.Business.Interfaces
{
    public interface IUserService
    {
        Task<int> CreateAsync(
            CreateUserDto dto);

        Task<IEnumerable<UserDto>> GetAllAsync();

        Task<UserDto?> GetByIdAsync(
            int id);

        Task<bool> UpdateAsync(
            int id,
            UpdateUserDto dto);

        Task<bool> PutDepartmentAsync(
           int Userid,
           int DepID);

        Task<bool> DeactivateAsync(
            int id);

        Task<bool> ActivateAsync(
            int id);

        Task<bool> ChangeMyPasswordAsync(
            int userId,
            ChangeMyPasswordDto dto);

        Task<bool> AdminChangePasswordAsync(
            int userId,
            AdminChangePasswordDto dto);
    }
}