namespace ComplaintManagement.Business.DTOs.Assignments
{
    public class UpdateAssignmentDto
    {
        public int AssignedToUserId { get; set; }

        public DateTime? UnassignedAt { get; set; }

        public string? Notes { get; set; }
    }
}