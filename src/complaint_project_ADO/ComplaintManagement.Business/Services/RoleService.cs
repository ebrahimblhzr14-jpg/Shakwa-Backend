using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using ComplaintManagement.Business.DTOs.Roles;
using ComplaintManagement.Business.Interfaces;

namespace ComplaintManagement.Business.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRolesRepository _repository;

        public RoleService(IRolesRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(
            CreateRoleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new Exception("Role name is required.");

            Role role = new Role
            {
                Name = dto.Name.Trim()
            };

            return await _repository.CreateAsync(role);
        }

        public async Task<IEnumerable<RoleDto>> GetAllAsync()
        {
            var roles = await _repository.GetAllAsync();

            return roles.Select(MapToDto);
        }

        public async Task<RoleDto?> GetByIdAsync(int id)
        {
            var role = await _repository.GetByIdAsync(id);

            if (role == null)
                return null;

            return MapToDto(role);
        }

        public async Task<bool> UpdateAsync(
            int id,
            UpdateRoleDto dto)
        {
            var role = await _repository.GetByIdAsync(id);

            if (role == null)
                return false;

            role.Name = dto.Name.Trim();

            return await _repository.UpdateAsync(role);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var role = await _repository.GetByIdAsync(id);

            if (role == null)
                return false;

            return await _repository.DeleteAsync(id);
        }

        private static RoleDto MapToDto(Role role)
        {
            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name
            };
        }
    }
}