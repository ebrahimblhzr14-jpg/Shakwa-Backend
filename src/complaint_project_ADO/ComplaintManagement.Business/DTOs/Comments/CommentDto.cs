namespace ComplaintManagement.Business.DTOs.Comments
{
    public class CommentDto
    {
        public int Id { get; set; }

        public int ComplaintId { get; set; }

        public int UserId { get; set; }

        public string? UserName { get; set; }

        public string? UserImageUrl { get; set; }

        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}