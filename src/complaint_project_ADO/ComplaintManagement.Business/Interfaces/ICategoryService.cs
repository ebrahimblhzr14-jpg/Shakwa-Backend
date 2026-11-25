using ComplaintManagement.Business.DTOs.Categories;

namespace ComplaintManagement.Business.Interfaces
{
    public interface ICategoryService
    {
        Task<int> CreateAsync(CreateCategoryDto dto);

        Task<IEnumerable<CategoryDto>> GetAllAsync();

        Task<CategoryDto?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(
            int id,
            UpdateCategoryDto dto);

        Task<bool> DeleteAsync(int id);

        Task<bool> ActivateAsync(int id);

        Task<bool> DeactivateAsync(int id);

    }
}