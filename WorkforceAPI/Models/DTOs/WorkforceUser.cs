using System.ComponentModel.DataAnnotations;

namespace WorkforceAPI.Models
{
    public class WorkforceUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        
        public int? EmployeeId { get; set; } 
        public string? Position { get; set; }
    }
}