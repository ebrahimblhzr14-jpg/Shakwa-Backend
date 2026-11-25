namespace ComplaintManagement.Business.DTOs.Assignments
{
    public class AssignmentDto
    {
        public int Id { get; set; }

        public int ComplaintId { get; set; }

        public int AssignedToUserId { get; set; }

        public string? AssignedToUserName { get; set; }

        public string? AssignedToUserImageUrl { get; set; }

        public int AssignedByUserId { get; set; }

        public string? AssignedByUserName { get; set; }

        public string? AssignedByUserImageUrl { get; set; }

        public DateTime AssignedAt { get; set; }

        public DateTime? UnassignedAt { get; set; }

        public string? Notes { get; set; }
    }
}