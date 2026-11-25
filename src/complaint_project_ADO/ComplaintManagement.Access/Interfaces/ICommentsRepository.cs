using ComplaintManagement.Access.Models;

namespace ComplaintManagement.Access.Interfaces
{
    public interface ICommentsRepository
    {
        Task<int> CreateAsync(ComplaintComment comment);

        Task<IEnumerable<ComplaintComment>>
            GetByComplaintAsync(int complaintId);

        Task<ComplaintComment?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(ComplaintComment comment);

        Task<bool> DeleteAsync(int id);
    }
}