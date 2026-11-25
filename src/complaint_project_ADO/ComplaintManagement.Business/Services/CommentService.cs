using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using ComplaintManagement.Business.DTOs.Comments;
using ComplaintManagement.Business.Interfaces;

namespace ComplaintManagement.Business.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentsRepository _repository;

        public CommentService(
            ICommentsRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(
            int userId,
            CreateCommentDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Comment))
                throw new Exception(
                    "Comment is required.");

            ComplaintComment comment =
                new ComplaintComment
                {
                    ComplaintId = dto.ComplaintId,
                    UserId = userId,
                    Comment = dto.Comment.Trim()
                };

            return await _repository.CreateAsync(comment);
        }

        public async Task<IEnumerable<CommentDto>>
            GetByComplaintAsync(int complaintId)
        {
            var comments =
                await _repository
                    .GetByComplaintAsync(complaintId);

            return comments.Select(MapToDto);
        }

        public async Task<CommentDto?> GetByIdAsync(int id)
        {
            var comment =
                await _repository.GetByIdAsync(id);

            return comment == null
                ? null
                : MapToDto(comment);
        }

        public async Task<bool> UpdateAsync(
            int id,
            int userId,
            UpdateCommentDto dto)
        {
            var comment =
                await _repository.GetByIdAsync(id);

            if (comment == null)
                return false;

            if (comment.UserId != userId)
                throw new UnauthorizedAccessException(
                    "You can only update your own comment.");

            comment.Comment =
                dto.Comment.Trim();

            return await _repository.UpdateAsync(
                comment);
        }

        public async Task<bool> DeleteAsync(
            int id,
            int userId)
        {
            var comment =
                await _repository.GetByIdAsync(id);

            if (comment == null)
                return false;

            if (comment.UserId != userId)
                throw new UnauthorizedAccessException(
                    "You can only delete your own comment.");

            return await _repository.DeleteAsync(id);
        }

        private static CommentDto MapToDto(
            ComplaintComment comment)
        {
            return new CommentDto
            {
                Id = comment.Id,
                ComplaintId = comment.ComplaintId,
                UserId = comment.UserId,
                UserName = comment.UserName,
                UserImageUrl = comment.UserImageUrl,
                Comment = comment.Comment,
                CreatedAt = comment.CreatedAt
            };
        }
    }
}