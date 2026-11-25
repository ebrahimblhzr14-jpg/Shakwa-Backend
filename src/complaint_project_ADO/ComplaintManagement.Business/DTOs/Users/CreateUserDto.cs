using System.ComponentModel.DataAnnotations;

namespace ComplaintManagement.Business.DTOs.Users
{
    public class CreateUserDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(2048)]
        public string? ImageUrl { get; set; }

        [Range(1,3)]
        public int RoleId { get; set; }
    }
}