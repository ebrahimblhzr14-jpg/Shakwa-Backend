using ComplaintManagement.Business.DTOs.Attachments;

namespace ComplaintManagement.Business.Interfaces
{
    public interface IAttachmentService
    {
        Task<int> CreateAsync(
            int userId,
            CreateAttachmentDto dto);

        Task<List<AttachmentDto>>
            GetByComplaintAsync(int complaintId);

        Task<AttachmentDto?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(
            int id,
            UpdateAttachmentDto dto);

        Task<bool> DeleteAsync(
            int id);
    }
}