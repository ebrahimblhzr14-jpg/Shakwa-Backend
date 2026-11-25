namespace ComplaintManagement.Business.DTOs.Users
{
    public class UpdateUserDto
    {
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        [System.ComponentModel.DataAnnotations.StringLength(2048)]
        public string? ImageUrl { get; set; }

        public int RoleId { get; set; }

        public bool IsActive { get; set; }
    }
}