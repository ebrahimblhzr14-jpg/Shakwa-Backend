
using ComplaintManagement.Business.DTOs.Priorities;

namespace ComplaintManagement.Business.Interfaces
{
    public interface IPriorityService
    {
        Task<int> CreateAsync(CreatePriorityDto dto);

        Task<IEnumerable<PriorityDto>> GetAllAsync();

        Task<PriorityDto?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(
            int id,
            UpdatePriorityDto dto);

        Task<bool> DeleteAsync(int id);

        Task<bool> ActivateAsync(int id);

        Task<bool> DeactivateAsync(int id);
    }
}


