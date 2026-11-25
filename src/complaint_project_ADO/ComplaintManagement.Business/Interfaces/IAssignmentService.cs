using ComplaintManagement.Business.DTOs.Assignments;

namespace ComplaintManagement.Business.Interfaces
{
    public interface IAssignmentService
    {
        Task<int> CreateAsync(
            int assignedByUserId,
            CreateAssignmentDto dto);

        Task<IEnumerable<AssignmentDto>> GetAllAsync();

        Task<AssignmentDto?> GetByIdAsync(int id);

        Task<bool> UpdateAsync(
            int id,
            UpdateAssignmentDto dto);

        Task<bool> DeleteAsync(int id);
    }
}