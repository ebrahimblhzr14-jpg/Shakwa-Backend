using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Models;
using ComplaintManagement.Business.DTOs.Attachments;
using ComplaintManagement.Business.Interfaces;

namespace ComplaintManagement.Business.Services
{
    public class AttachmentService : IAttachmentService
    {
        private readonly IAttachmentsRepository _repository;

        public AttachmentService(
            IAttachmentsRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> CreateAsync(
            int userId,
            CreateAttachmentDto dto)
        {
            if (dto.ComplaintId <= 0)
                throw new Exception(
                    "Invalid complaint.");

            if (string.IsNullOrWhiteSpace(dto.FileName))
                throw new Exception(
                    "File name is required.");

            ComplaintAttachment attachment =
                new ComplaintAttachment
                {
                    ComplaintId = dto.ComplaintId,
                    UploadedByUserId = userId,
                    FileName = dto.FileName,
                    FilePath = dto.FilePath,
                    FileType = dto.FileType,
                    FileSize = dto.FileSize
                };

            return await _repository.CreateAsync(
                attachment);
        }

        public async Task<List<AttachmentDto>>
            GetByComplaintAsync(int complaintId)
        {
            var attachments =
                await _repository
                    .GetByComplaintAsync(complaintId);

            return attachments.Select(MapToDto).ToList();
        }

        public async Task<AttachmentDto?> GetByIdAsync(
            int id)
        {
            var attachment =
                await _repository.GetByIdAsync(id);

            return attachment == null
                ? null
                : MapToDto(attachment);
        }

        public async Task<bool> UpdateAsync(
            int id,
            UpdateAttachmentDto dto)
        {
            var attachment =
                await _repository.GetByIdAsync(id);

            if (attachment == null)
                return false;

            attachment.FileName = dto.FileName;
            attachment.FilePath = dto.FilePath;
            attachment.FileType = dto.FileType;
            attachment.FileSize = dto.FileSize;

            return await _repository.UpdateAsync(
                attachment);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var attachment =
                await _repository.GetByIdAsync(id);

            if (attachment == null)
                return false;

            return await _repository.DeleteAsync(id);
        }

        private static AttachmentDto MapToDto(
            ComplaintAttachment attachment)
        {
            return new AttachmentDto
            {
                Id = attachment.Id,
                ComplaintId = attachment.ComplaintId,
                UploadedByUserId =
                    attachment.UploadedByUserId,
                UploadedByUserName =
                    attachment.UploadedByUserName,
                UploadedByUserImageUrl =
                    attachment.UploadedByUserImageUrl,
                FileName = attachment.FileName,
                FilePath = attachment.FilePath,
                FileType = attachment.FileType,
                FileSize = attachment.FileSize,
                UploadedAt = attachment.UploadedAt
            };
        }
    }
}