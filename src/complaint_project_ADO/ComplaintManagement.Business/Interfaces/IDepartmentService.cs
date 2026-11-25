using ComplaintManagement.Business.DTOs.Departments;

namespace ComplaintManagement.Business.Interfaces
{
    public interface IDepartmentService
    {
        Task<int> CreateAsync(CreateDepartmentDto dto);

        Task<IEnumerable<DepartmentDto>> GetAllAsync();

        Task<DepartmentDto?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(
            int id,
            UpdateDepartmentDto dto);

        Task<bool> DeleteAsync(int id);
    }
}