using ComplaintManagement.Access.Models;

namespace ComplaintManagement.Access.Interfaces
{
    public interface IComplaintsRepository
    {
        Task<int> CreateAsync(Complaint complaint);

        Task<IEnumerable<Complaint>> GetAllAsync();

        Task<Complaint?> GetByIdAsync(int id);

        Task<IEnumerable<Complaint>> GetByUserAsync(int userId);

        Task<IEnumerable<Complaint>> GetAssignedToAdminAsync(int adminId);

        Task<bool> UpdateAsync(Complaint complaint);

        Task<bool> DeleteAsync(int id);

        Task<bool> UpdateStatusAsync(
            int complaintId,
            int statusId,
            int userId);
    }
}
