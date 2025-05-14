using System.ComponentModel.DataAnnotations;

namespace WorkforceAPI.Models
{
    public class HRUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

       
        public string? Department { get; set; }
        public bool CanManageEmployees { get; set; } = true;
    }
}