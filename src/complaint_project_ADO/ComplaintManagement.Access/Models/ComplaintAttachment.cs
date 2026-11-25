namespace ComplaintManagement.Access.Models
{
    public class ComplaintAttachment
    {
        public int Id { get; set; }

        public int ComplaintId { get; set; }

        public int UploadedByUserId { get; set; }

        public string? UploadedByUserName { get; set; }

        public string? UploadedByUserImageUrl { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public string? FileType { get; set; }

        public long? FileSize { get; set; }

        public DateTime UploadedAt { get; set; }
    }
}