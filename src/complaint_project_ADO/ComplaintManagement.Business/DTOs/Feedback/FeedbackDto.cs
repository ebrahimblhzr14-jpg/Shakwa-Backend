namespace ComplaintManagement.Business.DTOs.Feedback
{
    public class FeedbackDto
    {
        public int Id { get; set; }

        public int ComplaintId { get; set; }

        public int UserId { get; set; }

        public string? UserName { get; set; }

        public string? UserImageUrl { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}