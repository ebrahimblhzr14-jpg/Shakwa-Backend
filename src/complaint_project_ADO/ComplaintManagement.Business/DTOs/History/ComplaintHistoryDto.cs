namespace ComplaintManagement.Business.DTOs.ComplaintHistory
{
    public class ComplaintHistoryDto
    {
        public int Id { get; set; }

        public int ComplaintId { get; set; }

        public int? UserId { get; set; }

        public string? UserName { get; set; }

        public string? UserImageUrl { get; set; }

        public string Action { get; set; } = string.Empty;

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}