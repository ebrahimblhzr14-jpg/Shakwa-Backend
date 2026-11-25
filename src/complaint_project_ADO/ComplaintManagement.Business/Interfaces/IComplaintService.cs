using ComplaintManagement.Business.DTOs.Complaints;
using ComplaintManagement.Business.Enums;

namespace ComplaintManagement.Business.Interfaces
{
    public interface IComplaintService
    {
        Task<int> CreateAsync(
            int userId,
            CreateComplaintDto dto);

        Task<IEnumerable<ComplaintDto>> GetAllAsync();

        Task<ComplaintDto?> GetByIdAsync(int id);

        Task<IEnumerable<ComplaintDto>> GetByUserAsync(
            int userId);

        Task<IEnumerable<ComplaintDto>> GetAssignedToAdminAsync(
            int adminId);

        Task<bool> UpdateAsync(
            int id,
            int userId,
            CreateComplaintDto dto);

        Task<bool> DeleteAsync(
            int id,
            int userId);

        Task<StatusUpdateResult> UpdateStatusAsync(
            int complaintId,
            int statusId,
            int userId);
    }
}
