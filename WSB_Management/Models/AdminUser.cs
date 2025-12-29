using System.ComponentModel.DataAnnotations;

namespace WSB_Management.Models
{
    public class AdminUser
    {
        public long Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        public bool IsSuperAdmin { get; set; } = false;
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? LastLogin { get; set; }
        
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
