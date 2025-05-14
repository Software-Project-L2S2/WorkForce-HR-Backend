using System;
using System.ComponentModel.DataAnnotations;

namespace WorkforceAPI.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeID { get; set; }

        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public string Category { get; set; } = "Full-Time";
        public string Gender { get; set; } = "Male";
        public string Email { get; set; } = string.Empty;
    }
}
