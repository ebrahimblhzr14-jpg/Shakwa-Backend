using ComplaintManagement.Business.DTOs.Comments;

namespace ComplaintManagement.Business.Interfaces
{
    public interface ICommentService
    {
        Task<int> CreateAsync(
            int userId,
            CreateCommentDto dto);

        Task<IEnumerable<CommentDto>>
            GetByComplaintAsync(int complaintId);

        Task<CommentDto?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(
            int id,
            int userId,
            UpdateCommentDto dto);

        Task<bool> DeleteAsync(
            int id,
            int userId);
    }
}