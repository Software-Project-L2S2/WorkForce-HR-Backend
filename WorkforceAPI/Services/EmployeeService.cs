using Microsoft.EntityFrameworkCore;
using WorkforceAPI.Data;
using WorkforceAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkforceAPI.Services
{
    public class EmployeeService
    {
        private readonly EmployeeContext _context;

        public EmployeeService(EmployeeContext context)
        {
            _context = context;
        }

        
     
        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            return await _context.Employees
                .AsNoTracking()
                .ToListAsync();
        }

        
        
      
        public async Task<IEnumerable<Employee>> GetFilteredEmployeesAsync(
            string? department, string? jobTitle, int? employeeID)
        {
            IQueryable<Employee> query = _context.Employees.AsQueryable();

            if (!string.IsNullOrWhiteSpace(department))
                query = query.Where(e => e.Department == department.Trim());

            if (!string.IsNullOrWhiteSpace(jobTitle))
                query = query.Where(e => e.JobTitle == jobTitle.Trim());

            if (employeeID.HasValue)
                query = query.Where(e => e.EmployeeID == employeeID.Value);

            return await query
                .AsNoTracking()
                .ToListAsync();
        }

        
        
        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            return await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeID == id);
        }

     
        
      
        public async Task<IEnumerable<DepartmentHeadCount>> GetDepartmentHeadCountsAsync()
        {
            return await _context.Employees
                .GroupBy(e => e.Department)
                .Select(g => new DepartmentHeadCount
                {
                    Department = g.Key ?? "Undefined",
                    Count = g.Count()
                })
                .AsNoTracking()
                .ToListAsync();
        }

    
        
      
        public async Task<IEnumerable<Employee>> SearchEmployeesAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Enumerable.Empty<Employee>();

            string normalizedTerm = term.Trim().ToLower();

            return await _context.Employees
                .Where(e =>
                    EF.Functions.Like(e.EmployeeName.ToLower(), $"%{normalizedTerm}%") ||
                    EF.Functions.Like(e.Department.ToLower(), $"%{normalizedTerm}%") ||
                    EF.Functions.Like(e.JobTitle.ToLower(), $"%{normalizedTerm}%"))
                .AsNoTracking()
                .ToListAsync();
        }

        
        
        
        public class DepartmentHeadCount
        {
            public string Department { get; set; } = "Undefined";
            public int Count { get; set; }
        }

        public async Task<List<Employee>> GetEmployeesByIdsAsync(List<int> employeeIds)
        {
            return await _context.Employees
                .Where(e => employeeIds.Contains(e.EmployeeID))
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
