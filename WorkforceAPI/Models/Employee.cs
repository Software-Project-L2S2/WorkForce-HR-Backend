using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkforceAPI.Models
{
    [Table("Employees")]
    public class Employee
    {
        [Key]
        [Column("EmployeeID")]
        public int EmployeeID { get; set; }

        [Required(ErrorMessage = "Employee name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        [Column("EmployeeName")]
        public string EmployeeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        [StringLength(50, ErrorMessage = "Department cannot exceed 50 characters")]
        [Column("Department")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Job title is required")]
        [StringLength(50, ErrorMessage = "Job title cannot exceed 50 characters")]
        [Column("JobTitle")]
        public string JobTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Column("StartDate")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [StringLength(20, ErrorMessage = "Category cannot exceed 20 characters")]
        [Column("Category")]
        public string Category { get; set; } = "Full-Time";

        [Required(ErrorMessage = "Gender is required")]
        [StringLength(10, ErrorMessage = "Gender cannot exceed 10 characters")]
        [Column("Gender")]
        public string Gender { get; set; } = "Male";

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
        [Column("Email")]
        public string Email { get; set; } = string.Empty;
    }
}