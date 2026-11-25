using ComplaintManagement.Business.DTOs.Dashboard;

namespace ComplaintManagement.Business.Interfaces
{
    public interface IDashboardService
    {
        Task<ComplaintSummaryDto>
            GetSummaryAsync();

        Task<IEnumerable<ComplaintsByStatusDto>>
            GetComplaintsByStatusAsync();

        Task<IEnumerable<ComplaintsByPriorityDto>>
            GetComplaintsByPriorityAsync();

        Task<IEnumerable<ComplaintsByCategoryDto>>
            GetComplaintsByCategoryAsync();
    }
}