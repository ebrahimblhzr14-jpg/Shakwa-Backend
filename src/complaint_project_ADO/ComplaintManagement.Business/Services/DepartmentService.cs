using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using ComplaintManagement.Business.DTOs.Departments;
using ComplaintManagement.Business.Interfaces;

namespace ComplaintManagement.Business.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentsRepository _repository;

        public DepartmentService(
            IDepartmentsRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(
            CreateDepartmentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new Exception(
                    "Department name is required.");

            Department department = new Department
            {
                Name = dto.Name.Trim(),
                Description = dto.Description
            };

            return await _repository.CreateAsync(department);
        }

        public async Task<IEnumerable<DepartmentDto>>
            GetAllAsync()
        {
            var departments =
                await _repository.GetAllAsync();

            return departments.Select(MapToDto);
        }

        public async Task<DepartmentDto?> GetByIdAsync(int id)
        {
            var department =
                await _repository.GetByIdAsync(id);

            return department == null
                ? null
                : MapToDto(department);
        }

        public async Task<bool> UpdateAsync(
            int id,
            UpdateDepartmentDto dto)
        {
            var department =
                await _repository.GetByIdAsync(id);

            if (department == null)
                return false;

            department.Name = dto.Name.Trim();
            department.Description = dto.Description;
            department.IsActive = dto.IsActive;

            return await _repository.UpdateAsync(
                department);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var department =
                await _repository.GetByIdAsync(id);

            if (department == null)
                return false;

            return await _repository.DeleteAsync(id);
        }

        private static DepartmentDto MapToDto(
            Department department)
        {
            return new DepartmentDto
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description,
                IsActive = department.IsActive
            };
        }
    }
}