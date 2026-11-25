

using ComplaintManagement.Business.DTOs.Statuses;

namespace ComplaintManagement.Business.Interfaces
{
    public interface IStatusService
    {
        Task<int> CreateAsync(CreateStatusDto dto);

        Task<IEnumerable<StatusDto>> GetAllAsync();

        Task<StatusDto?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(
            int id,
            UpdateStatusDto dto);

        Task<bool> DeleteAsync(int id);

        Task<bool> ActivateAsync(int id);

        Task<bool> DeactivateAsync(int id);
    }
}
