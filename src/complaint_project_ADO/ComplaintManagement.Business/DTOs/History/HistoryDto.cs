namespace ComplaintManagement.Business.DTOs.History
{
    public class HistoryDto
    {
        public int Id { get; set; }

        public int ComplaintId { get; set; }

        public int UserId { get; set; }

        public string? UserName { get; set; }

        public string? UserImageUrl { get; set; }

        public string Action { get; set; } = string.Empty;

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}