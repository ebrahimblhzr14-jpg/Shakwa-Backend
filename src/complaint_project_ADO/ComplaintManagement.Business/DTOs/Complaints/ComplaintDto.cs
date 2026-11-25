
namespace ComplaintManagement.Business.DTOs.Complaints
{
    public class ComplaintDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string? UserName { get; set; }

        public string? UserImageUrl { get; set; }

        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public int? DepartmentId { get; set; }

        public string? DepartmentName { get; set; }

        public int StatusId { get; set; }

        public string? StatusName { get; set; }

        public int PriorityId { get; set; }

        public string? PriorityName { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        public DateTime? ClosedAt { get; set; }
    }
}

