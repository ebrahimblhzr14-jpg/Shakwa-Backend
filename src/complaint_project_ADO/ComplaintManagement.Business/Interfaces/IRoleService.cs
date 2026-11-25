using ComplaintManagement.Business.DTOs.Roles;

namespace ComplaintManagement.Business.Interfaces
{
    public interface IRoleService
    {
        Task<int> CreateAsync(CreateRoleDto dto);

        Task<IEnumerable<RoleDto>> GetAllAsync();

        Task<RoleDto?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(
            int id,
            UpdateRoleDto dto);

        Task<bool> DeleteAsync(int id);
    }
}