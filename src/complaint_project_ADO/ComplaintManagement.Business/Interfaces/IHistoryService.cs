using ComplaintManagement.Business.DTOs.ComplaintHistory;

namespace ComplaintManagement.Business.Interfaces
{
    public interface IComplaintHistoryService
    {
        Task<IEnumerable<ComplaintHistoryDto>>
            GetByComplaintIdAsync(
                int complaintId);

        Task<IEnumerable<ComplaintHistoryDto>>
            GetAllGlobalAsync();

        Task<ComplaintHistoryDto?>
            GetByIdAsync(int id);

        Task<int>
            CreateAsync(
                int complaintId,
                int? userId,
                string action,
                string? oldValue,
                string? newValue,
                string? description);
    }
}
