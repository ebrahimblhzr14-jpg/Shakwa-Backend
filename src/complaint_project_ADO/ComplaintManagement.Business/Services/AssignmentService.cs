using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using ComplaintManagement.Business.DTOs.Assignments;
using ComplaintManagement.Business.Interfaces;

namespace ComplaintManagement.Business.Services
{
    public class AssignmentService : IAssignmentService
    {
        private readonly IAssignmentsRepository _repository;

        public AssignmentService(
            IAssignmentsRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(
            int assignedByUserId,
            CreateAssignmentDto dto)
        {
            if (dto.ComplaintId <= 0)
                throw new Exception(
                    "Invalid complaint.");

            if (dto.AssignedToUserId <= 0)
                throw new Exception(
                    "Invalid assigned user.");

            ComplaintAssignment assignment =
                new ComplaintAssignment
                {
                    ComplaintId = dto.ComplaintId,
                    AssignedToUserId =
                        dto.AssignedToUserId,
                    AssignedByUserId =
                        assignedByUserId,
                    Notes = dto.Notes
                };

            return await _repository.CreateAsync(
                assignment);
        }

        public async Task<IEnumerable<AssignmentDto>>
            GetAllAsync()
        {
            var assignments =
                await _repository.GetAllAsync();

            return assignments.Select(MapToDto);
        }

        public async Task<AssignmentDto?> GetByIdAsync(
            int id)
        {
            var assignment =
                await _repository.GetByIdAsync(id);

            return assignment == null
                ? null
                : MapToDto(assignment);
        }

        public async Task<bool> UpdateAsync(
            int id,
            UpdateAssignmentDto dto)
        {
            var assignment =
                await _repository.GetByIdAsync(id);

            if (assignment == null)
                return false;

            assignment.AssignedToUserId =
                dto.AssignedToUserId;

            assignment.UnassignedAt =
                dto.UnassignedAt;

            assignment.Notes =
                dto.Notes;

            return await _repository.UpdateAsync(
                assignment);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var assignment =
                await _repository.GetByIdAsync(id);

            if (assignment == null)
                return false;

            return await _repository.DeleteAsync(id);
        }

        private static AssignmentDto MapToDto(
            ComplaintAssignment assignment)
        {
            return new AssignmentDto
            {
                Id = assignment.Id,

                ComplaintId =
                    assignment.ComplaintId,

                AssignedToUserId =
                    assignment.AssignedToUserId,

                AssignedToUserName =
                    assignment.AssignedToUserName,

                AssignedToUserImageUrl =
                    assignment.AssignedToUserImageUrl,

                AssignedByUserId =
                    assignment.AssignedByUserId,

                AssignedByUserName =
                    assignment.AssignedByUserName,

                AssignedByUserImageUrl =
                    assignment.AssignedByUserImageUrl,

                AssignedAt =
                    assignment.AssignedAt,

                UnassignedAt =
                    assignment.UnassignedAt,

                Notes =
                    assignment.Notes
            };
        }
    }
}