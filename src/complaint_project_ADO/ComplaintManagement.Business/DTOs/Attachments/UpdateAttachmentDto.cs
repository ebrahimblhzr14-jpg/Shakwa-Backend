namespace ComplaintManagement.Business.DTOs.Attachments
{
    public class UpdateAttachmentDto
    {
        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public string? FileType { get; set; }

        public long? FileSize { get; set; }
    }
}