namespace ComplaintManagement.Business.DTOs.Complaints
{
    public class UpdateComplaintDto
    {
        public int CategoryId { get; set; }

        public int? DepartmentId { get; set; }

        public int PriorityId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}