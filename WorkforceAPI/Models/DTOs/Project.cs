namespace WorkforceAPI.Models
{
    public class Project
    {
        public int Id { get; set; } 
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public string RequiredSkills { get; set; } = string.Empty; 
        public List<ProjectEmployee> Employees { get; set; } = new();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ProjectEmployee
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty; 
    }

    public class ProjectAssignmentDto
    {
        public int ProjectId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
    }

    public class ProjectDto
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public List<string> RequiredSkills { get; set; } = new();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<int> EmployeeIds { get; set; } = new List<int>();
    }
}