using ComplaintManagement.Access.Models;

namespace ComplaintManagement.Access.Interfaces
{
    public interface IDashboardRepository
    {
        Task<DashboardSummary> GetSummaryAsync();

        Task<IEnumerable<ComplaintsByStatus>>
            GetComplaintsByStatusAsync();

        Task<IEnumerable<ComplaintsByPriority>>
            GetComplaintsByPriorityAsync();

        Task<IEnumerable<ComplaintsByCategory>>
            GetComplaintsByCategoryAsync();
    }
}