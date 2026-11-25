using ComplaintManagement.Access.Models;

namespace ComplaintManagement.Access.Interfaces
{
    public interface IFeedbackRepository
    {
        Task<int> CreateAsync(
            ComplaintFeedback feedback);

        Task<IEnumerable<ComplaintFeedback?>>
            GetByComplaintAsync(int complaintId);

        Task<ComplaintFeedback?>
            GetByIdAsync(int id);

        Task<bool> UpdateAsync(
            ComplaintFeedback feedback);

        Task<bool> DeleteAsync(int id);
    }
}