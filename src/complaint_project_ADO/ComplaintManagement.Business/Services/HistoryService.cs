using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using ComplaintManagement.Business.DTOs.ComplaintHistory;
using ComplaintManagement.Business.Interfaces;

namespace ComplaintManagement.Business.Services
{
    public class ComplaintHistoryService
        : IComplaintHistoryService
    {
        private readonly IComplaintHistoryRepository
            _historyRepository;

        public ComplaintHistoryService(
            IComplaintHistoryRepository historyRepository)
        {
            _historyRepository =
                historyRepository;
        }


        // =====================================================
        // Get By Complaint
        // =====================================================

        public async Task<IEnumerable<ComplaintHistoryDto>>
            GetByComplaintIdAsync(int complaintId)
        {
            if (complaintId <= 0)
            {
                throw new ArgumentException(
                    "Invalid complaint ID.");
            }

            var history =
                await _historyRepository
                    .GetByComplaintIdAsync(
                        complaintId);

            return history.Select(MapToDto);
        }


        // =====================================================
        // Get All Global (SuperAdmin Audit)
        // =====================================================

        public async Task<IEnumerable<ComplaintHistoryDto>>
            GetAllGlobalAsync()
        {
            var history =
                await _historyRepository
                    .GetAllGlobalAsync();

            return history.Select(MapToDto);
        }


        // =====================================================
        // Get By ID
        // =====================================================

        public async Task<ComplaintHistoryDto?>
            GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException(
                    "Invalid history ID.");
            }

            var history =
                await _historyRepository
                    .GetByIdAsync(id);

            if (history == null)
            {
                return null;
            }

            return MapToDto(history);
        }


        // =====================================================
        // Create
        // =====================================================

        public async Task<int> CreateAsync(
            int complaintId,
            int? userId,
            string action,
            string? oldValue,
            string? newValue,
            string? description)
        {
            if (complaintId <= 0)
            {
                throw new ArgumentException(
                    "Invalid complaint ID.");
            }

            if (string.IsNullOrWhiteSpace(action))
            {
                throw new ArgumentException(
                    "Action is required.");
            }

            var history = new ComplaintHistory
            {
                ComplaintId = complaintId,

                UserId = userId,

                Action = action,

                OldValue = oldValue,

                NewValue = newValue,

                Description = description
            };

            return await _historyRepository
                .CreateAsync(history);
        }


        // =====================================================
        // Mapper
        // =====================================================

        private static ComplaintHistoryDto
            MapToDto(ComplaintHistory history)
        {
            return new ComplaintHistoryDto
            {
                Id = history.Id,

                ComplaintId =
                    history.ComplaintId,

                UserId =
                    history.UserId,

                UserName =
                    history.UserName,

                UserImageUrl =
                    history.UserImageUrl,

                Action =
                    history.Action,

                OldValue =
                    history.OldValue,

                NewValue =
                    history.NewValue,

                Description =
                    history.Description,

                CreatedAt =
                    history.CreatedAt
            };
        }
    }
}