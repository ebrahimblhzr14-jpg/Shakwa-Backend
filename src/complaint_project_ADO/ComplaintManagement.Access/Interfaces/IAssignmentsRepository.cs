using ComplaintManagement.Access.Models;

namespace ComplaintManagement.Access.Interfaces
{
    public interface IAssignmentsRepository
    {
        Task<int> CreateAsync(
            ComplaintAssignment assignment);

        Task<IEnumerable<ComplaintAssignment>>
            GetAllAsync();

        Task<ComplaintAssignment?> GetByIdAsync(
            int id);

        Task<bool> UpdateAsync(
            ComplaintAssignment assignment);

        Task<bool> DeleteAsync(int id);

        Task<bool> HasActiveAssignmentAsync(int complaintId, int adminId);
    }
}
