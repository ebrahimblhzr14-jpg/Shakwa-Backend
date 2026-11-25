using ComplaintManagement.Access.Models;

namespace ComplaintManagement.Access.Interfaces
{
    public interface IStatusesRepository
    {
        Task<int> CreateAsync(ComplaintStatus status);

        Task<IEnumerable<ComplaintStatus>> GetAllAsync();

        Task<ComplaintStatus?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(ComplaintStatus status);

        Task<bool> DeleteAsync(int id);
        Task<bool> ActivateAsync(int id);

        Task<bool> DeactivateAsync(int id);
    }
}