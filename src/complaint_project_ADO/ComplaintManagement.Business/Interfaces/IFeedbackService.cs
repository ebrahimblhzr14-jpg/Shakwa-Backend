using ComplaintManagement.Business.DTOs.Feedback;

namespace ComplaintManagement.Business.Interfaces
{
    public interface IFeedbackService
    {
        Task<int> CreateAsync(
            int userId,
            CreateFeedbackDto dto);

        Task<IEnumerable<FeedbackDto?>>
            GetByComplaintAsync(int complaintId);

        Task<FeedbackDto?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(
            int id,
            int userId,
            UpdateFeedbackDto dto);

        Task<bool> DeleteAsync(
            int id,
            int userId);
    }
}