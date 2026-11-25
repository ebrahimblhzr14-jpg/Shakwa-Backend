namespace ComplaintManagement.Business.DTOs.Assignments
{
    public class CreateAssignmentDto
    {
        public int ComplaintId { get; set; }

        public int AssignedToUserId { get; set; }

        public string? Notes { get; set; }
    }
}