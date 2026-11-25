using ComplaintManagement.Access.Models;

namespace ComplaintManagement.Access.Interfaces
{
    public interface IAttachmentsRepository
    {
        Task<int> CreateAsync(
            ComplaintAttachment attachment);

        Task<IEnumerable<ComplaintAttachment>>
            GetByComplaintAsync(int complaintId);

        Task<ComplaintAttachment?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(
            ComplaintAttachment attachment);

        Task<bool> DeleteAsync(int id);
    }
}