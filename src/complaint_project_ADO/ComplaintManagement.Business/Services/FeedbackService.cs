using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using ComplaintManagement.Business.DTOs.Feedback;
using ComplaintManagement.Business.Interfaces;

namespace ComplaintManagement.Business.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _repository;

        public FeedbackService(
            IFeedbackRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(
            int userId,
            CreateFeedbackDto dto)
        {
            if (dto.Rating <= 1 && dto.Rating > 5)
                throw new Exception(
                    "Rating must be between 1 and 5.");


            ComplaintFeedback feedback =
                new ComplaintFeedback
                {
                    ComplaintId = dto.ComplaintId,
                    UserId = userId,
                    Rating = dto.Rating,
                    Comment = dto.Comment
                };

            return await _repository.CreateAsync(
                feedback);
        }

        public async Task<IEnumerable<FeedbackDto?>>
            GetByComplaintAsync(int complaintId)
        {
            var feedback =
                await _repository
                    .GetByComplaintAsync(
                        complaintId);

            return feedback.Select(MapToDto);


        }

        public async Task<FeedbackDto?> GetByIdAsync(
            int id)
        {
            var feedback =
                await _repository.GetByIdAsync(id);

            return feedback == null
                ? null
                : MapToDto(feedback);
        }

        public async Task<bool> UpdateAsync(
            int id,
            int userId,
            UpdateFeedbackDto dto)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
                throw new Exception(
                    "Rating must be between 1 and 5.");

            var feedback =
                await _repository.GetByIdAsync(id);

            if (feedback == null)
                return false;

            if (feedback.UserId != userId)
                throw new UnauthorizedAccessException(
                    "You can only update your own feedback.");

            feedback.Rating = dto.Rating;
            feedback.Comment = dto.Comment;

            return await _repository.UpdateAsync(
                feedback);
        }

        public async Task<bool> DeleteAsync(
            int id,
            int userId)
        {
            var feedback =
                await _repository.GetByIdAsync(id);

            if (feedback == null)
                return false;

            if (feedback.UserId != userId)
                throw new UnauthorizedAccessException(
                    "You can only delete your own feedback.");

            return await _repository.DeleteAsync(id);
        }

        private static FeedbackDto MapToDto(
            ComplaintFeedback feedback)
        {
            return new FeedbackDto
            {
                Id = feedback.Id,
                ComplaintId = feedback.ComplaintId,
                UserId = feedback.UserId,
                UserName = feedback.UserName,
                UserImageUrl = feedback.UserImageUrl,
                Rating = feedback.Rating,
                Comment = feedback.Comment,
                CreatedAt = feedback.CreatedAt
            };
        }
    }
}