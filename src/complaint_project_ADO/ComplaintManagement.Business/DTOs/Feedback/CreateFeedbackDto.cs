namespace ComplaintManagement.Business.DTOs.Feedback
{
    public class CreateFeedbackDto
    {
        public int ComplaintId { get; set; }

        public int Rating { get; set; }

        public string? Comment { get; set; }
    }
}