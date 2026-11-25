using ComplaintManagement.Access.Models;

namespace ComplaintManagement.Access.Interfaces
{
    public interface IComplaintHistoryRepository
    {
        Task<IEnumerable<ComplaintHistory>>
            GetByComplaintIdAsync(int complaintId);

        Task<IEnumerable<ComplaintHistory>>
            GetAllGlobalAsync();

        Task<ComplaintHistory?>
            GetByIdAsync(int id);

        Task<int>
            CreateAsync(ComplaintHistory history);
    }
}
