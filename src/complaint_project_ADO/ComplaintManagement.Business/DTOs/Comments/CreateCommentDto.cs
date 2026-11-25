
namespace ComplaintManagement.Business.DTOs.Comments
{
    public class CreateCommentDto
    {
        public int ComplaintId { get; set; }

        public string Comment { get; set; } = string.Empty;
    }
}
