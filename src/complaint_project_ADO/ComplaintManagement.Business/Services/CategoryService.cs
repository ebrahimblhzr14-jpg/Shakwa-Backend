using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using ComplaintManagement.Business.DTOs.Categories;
using ComplaintManagement.Business.Interfaces;

namespace ComplaintManagement.Business.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoriesRepository _repository;

        public CategoryService(
            ICategoriesRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(
            CreateCategoryDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new Exception(
                    "Category name is required.");

            ComplaintCategory category =
                new ComplaintCategory
                {
                    Name = dto.Name.Trim(),
                    Description = dto.Description
                };

            return await _repository.CreateAsync(category);
        }

        public async Task<IEnumerable<CategoryDto>>
            GetAllAsync()
        {
            var categories =
                await _repository.GetAllAsync();

            return categories.Select(MapToDto);
        }

        public async Task<CategoryDto?> GetByIdAsync(int id)
        {
            var category =
                await _repository.GetByIdAsync(id);

            return category == null
                ? null
                : MapToDto(category);
        }

        public async Task<bool> UpdateAsync(
            int id,
            UpdateCategoryDto dto)
        {
            var category =
                await _repository.GetByIdAsync(id);

            if (category == null)
                return false;

            category.Name = dto.Name.Trim();
            category.Description = dto.Description;
            category.IsActive = dto.IsActive;

            return await _repository.UpdateAsync(
                category);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category =
                await _repository.GetByIdAsync(id);

            if (category == null)
                return false;

            return await _repository.DeleteAsync(id);
        }

        private static CategoryDto MapToDto(
            ComplaintCategory category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive
            };
        }

        public async Task<bool> ActivateAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "Invalid category ID.");
            }

            return await _repository
                .ActivateAsync(id);
        }
        public async Task<bool> DeactivateAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "Invalid category ID.");
            }

            return await _repository
                .DeactivateAsync(id);
        }

    }
}