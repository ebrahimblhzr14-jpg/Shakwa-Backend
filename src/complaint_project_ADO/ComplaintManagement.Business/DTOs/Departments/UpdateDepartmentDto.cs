namespace ComplaintManagement.Business.DTOs.Departments
{
    public class UpdateDepartmentDto
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}