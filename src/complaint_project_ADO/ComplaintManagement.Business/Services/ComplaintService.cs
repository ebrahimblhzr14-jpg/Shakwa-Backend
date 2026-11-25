using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using ComplaintManagement.Business.DTOs.Complaints;
using ComplaintManagement.Business.Enums;
using ComplaintManagement.Business.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading;

namespace ComplaintManagement.Business.Services
{
    public class ComplaintService : IComplaintService
    {
        private readonly IComplaintsRepository _repository;
        private readonly ILogger<ComplaintService> _logger;

        // Shared across all scoped/transient instances within the process to serialize status updates per complaint
        private static readonly ConcurrentDictionary<int, SemaphoreSlim> _complaintLocks = new();

        public ComplaintService(ILogger<ComplaintService> logger,
            IComplaintsRepository repository)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<int> CreateAsync(
            int userId,
            CreateComplaintDto dto)
        {
            _logger.LogInformation(
                "Creating complaint for user {UserId}",
                userId);
            ValidateComplaint(dto);

            Complaint complaint = new Complaint
            {
                UserId = userId,
                CategoryId = dto.CategoryId,
                DepartmentId = dto.DepartmentId,
                PriorityId = dto.PriorityId,

                // New complaint
                StatusId = 1,

                Title = dto.Title.Trim(),
                Description = dto.Description.Trim()
            };

            int comID = await _repository.CreateAsync(
                complaint);
            if (comID > 0)
            {
                _logger.LogInformation(
                    "Complaint {ComplaintId} created successfully for user {UserId}",
                    comID,
                    userId);
                return comID;
            }

            return 0;
        }

        public async Task<IEnumerable<ComplaintDto>>
            GetAllAsync()
        {
            var complaints =
                await _repository.GetAllAsync();

            return complaints.Select(MapToDto);
        }

        public async Task<ComplaintDto?> GetByIdAsync(
            int id)
        {
            var complaint =
                await _repository.GetByIdAsync(id);
            if (complaint == null)
            {
                _logger.LogWarning(
                    "Complaint {ComplaintId} was not found",
                    id);
            }

            return complaint == null
                ? null
                : MapToDto(complaint);
        }

        public async Task<IEnumerable<ComplaintDto>>
            GetByUserAsync(int userId)
        {
            _logger.LogInformation(
                "all complaint for user {UserId}",
                userId);
            var complaints =
                await _repository.GetByUserAsync(userId);

            return complaints.Select(MapToDto);
        }

        public async Task<IEnumerable<ComplaintDto>>
            GetAssignedToAdminAsync(int adminId)
        {
            _logger.LogInformation(
                "Retrieving assigned complaints for admin {AdminId}",
                adminId);
            var complaints =
                await _repository.GetAssignedToAdminAsync(adminId);

            return complaints.Select(MapToDto);
        }

        public async Task<bool> UpdateAsync(
            int id,
            int userId,
            CreateComplaintDto dto)
        {
            ValidateComplaint(dto);

            var complaint =
                await _repository.GetByIdAsync(id);

            if (complaint == null)
                return false;

            // Ownership check
            if (complaint.UserId != userId)
                throw new UnauthorizedAccessException(
                    "You can only update your own complaint.");

            complaint.CategoryId = dto.CategoryId;
            complaint.DepartmentId = dto.DepartmentId;
            complaint.PriorityId = dto.PriorityId;
            complaint.Title = dto.Title.Trim();
            complaint.Description = dto.Description.Trim();

            return await _repository.UpdateAsync(
                complaint);
        }

        public async Task<bool> DeleteAsync(
            int id,
            int userId)
        {
            var complaint =
                await _repository.GetByIdAsync(id);

            if (complaint == null)
                return false;

            if (complaint.UserId != userId)
                throw new UnauthorizedAccessException(
                    "You can only delete your own complaint.");

            return await _repository.DeleteAsync(id);
        }

        public async Task<StatusUpdateResult> UpdateStatusAsync(
            int complaintId,
            int statusId,
            int userId)
        {
            if (complaintId <= 0)
                return StatusUpdateResult.NotFound;

            if (statusId < 1 || statusId > 5)
                return StatusUpdateResult.InvalidTransition;

            // Acquire per-complaint lock shared across all requests in this process
            var semaphore = _complaintLocks.GetOrAdd(complaintId, _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();

            try
            {
                // Re-read complaint state after acquiring lock immediately before executing SP
                var complaint = await _repository.GetByIdAsync(complaintId);
                if (complaint == null)
                {
                    return StatusUpdateResult.NotFound;
                }

                // Guard: If complaint is already in terminal Closed status
                if (complaint.StatusId == 5 || complaint.ClosedAt != null)
                {
                    if (statusId == 5)
                    {
                        return StatusUpdateResult.AlreadyClosed;
                    }
                    return StatusUpdateResult.CannotModifyClosed;
                }

                // Call existing stored procedure via repository
                bool updated = await _repository.UpdateStatusAsync(
                    complaintId,
                    statusId,
                    userId);

                return updated
                    ? StatusUpdateResult.Success
                    : StatusUpdateResult.NotFound;
            }
            finally
            {
                semaphore.Release();
            }
        }

        private static void ValidateComplaint(
            CreateComplaintDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new Exception(
                    "Complaint title is required.");

            if (string.IsNullOrWhiteSpace(dto.Description))
                throw new Exception(
                    "Complaint description is required.");

            if (dto.CategoryId <= 0)
                throw new Exception(
                    "Invalid category.");

            if (dto.PriorityId <= 0)
                throw new Exception(
                    "Invalid priority.");
        }
       
        private static ComplaintDto MapToDto(
            Complaint complaint)
        {
            return new ComplaintDto
            {
                Id = complaint.Id,

                UserId = complaint.UserId,
                UserName = complaint.UserName,
                UserImageUrl = complaint.UserImageUrl,

                CategoryId = complaint.CategoryId,
                CategoryName = complaint.CategoryName,

                DepartmentId = complaint.DepartmentId,
                DepartmentName = complaint.DepartmentName,

                StatusId = complaint.StatusId,
                StatusName = complaint.StatusName,

                PriorityId = complaint.PriorityId,
                PriorityName = complaint.PriorityName,

                Title = complaint.Title,
                Description = complaint.Description,

                CreatedAt = complaint.CreatedAt,
                UpdatedAt = complaint.UpdatedAt,
                ResolvedAt = complaint.ResolvedAt,
                ClosedAt = complaint.ClosedAt
            };
        }
    }
}
