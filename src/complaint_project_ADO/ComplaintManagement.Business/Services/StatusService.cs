using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using ComplaintManagement.Business.DTOs.Statuses;
using ComplaintManagement.Business.Interfaces;

namespace ComplaintManagement.Business.Services
{
    public class StatusService : IStatusService
    {
        private readonly IStatusesRepository _repository;

        public StatusService(
            IStatusesRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(
            CreateStatusDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new Exception(
                    "Status name is required.");

            ComplaintStatus status =
                new ComplaintStatus
                {
                    Name = dto.Name.Trim()
                };

            return await _repository.CreateAsync(status);
        }

        public async Task<IEnumerable<StatusDto>>
            GetAllAsync()
        {
            var statuses =
                await _repository.GetAllAsync();

            return statuses.Select(MapToDto);
        }

        public async Task<StatusDto?> GetByIdAsync(int id)
        {
            var status =
                await _repository.GetByIdAsync(id);

            return status == null
                ? null
                : MapToDto(status);
        }

        public async Task<bool> UpdateAsync(
            int id,
            UpdateStatusDto dto)
        {
            var status =
                await _repository.GetByIdAsync(id);

            if (status == null)
                return false;

            status.Name = dto.Name.Trim();

            return await _repository.UpdateAsync(status);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var status =
                await _repository.GetByIdAsync(id);

            if (status == null)
                return false;

            return await _repository.DeleteAsync(id);
        }

        public async Task<bool> ActivateAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "Invalid status ID.");
            }

            return await _repository
                .ActivateAsync(id);
        }

        public async Task<bool> DeactivateAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "Invalid status ID.");
            }

            return await _repository
                .DeactivateAsync(id);
        }


        private static StatusDto MapToDto(
            ComplaintStatus status)
        {
            return new StatusDto
            {
                Id = status.Id,
                Name = status.Name
            };
        }
    }
}