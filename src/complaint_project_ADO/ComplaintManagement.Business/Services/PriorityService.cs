using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using ComplaintManagement.Business.DTOs.Priorities;
using ComplaintManagement.Business.Interfaces;

namespace ComplaintManagement.Business.Services
{
    public class PriorityService : IPriorityService
    {
        private readonly IPrioritiesRepository _repository;

        public PriorityService(
            IPrioritiesRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(
            CreatePriorityDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new Exception(
                    "Priority name is required.");

            if (dto.Level < 1)
                throw new Exception(
                    "Priority level must be greater than zero.");

            Priority priority = new Priority
            {
                Name = dto.Name.Trim(),
                Level = dto.Level
            };

            return await _repository.CreateAsync(priority);
        }

        public async Task<IEnumerable<PriorityDto>>
            GetAllAsync()
        {
            var priorities =
                await _repository.GetAllAsync();

            return priorities.Select(MapToDto);
        }

        public async Task<PriorityDto?> GetByIdAsync(int id)
        {
            var priority =
                await _repository.GetByIdAsync(id);

            return priority == null
                ? null
                : MapToDto(priority);
        }

        public async Task<bool> UpdateAsync(
            int id,
            UpdatePriorityDto dto)
        {
            var priority =
                await _repository.GetByIdAsync(id);

            if (priority == null)
                return false;

            priority.Name = dto.Name.Trim();
            priority.Level = dto.Level;

            return await _repository.UpdateAsync(priority);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var priority =
                await _repository.GetByIdAsync(id);

            if (priority == null)
                return false;

            return await _repository.DeleteAsync(id);
        }



        public async Task<bool> ActivateAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "Invalid priority ID.");
            }

            return await _repository
                .ActivateAsync(id);
        }



        public async Task<bool> DeactivateAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "Invalid priority ID.");
            }

            return await _repository
                .DeactivateAsync(id);
        }



        private static PriorityDto MapToDto(
            Priority priority)
        {
            return new PriorityDto
            {
                Id = priority.Id,
                Name = priority.Name,
                Level = priority.Level
            };
        }
    }
}