namespace ComplaintManagement.Business.DTOs.Users
{
    public class UserDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public string? ImageUrl { get; set; }

        public int RoleId { get; set; }

        public string? RoleName { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}