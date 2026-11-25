namespace ComplaintManagement.Business.DTOs.History
{
    public class CreateHistoryDto
    {
        public int ComplaintId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }
    }
}