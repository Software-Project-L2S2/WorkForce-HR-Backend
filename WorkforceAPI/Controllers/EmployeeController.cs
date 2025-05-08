using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WorkforceAPI.Models;
using WorkforceAPI.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkforceAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly EmployeeService _service;
        private readonly ILogger<EmployeesController> _logger;

        public EmployeesController(
            EmployeeService service,
            ILogger<EmployeesController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // GET: api/employees (HR & Admin)
        [Authorize(Roles = "HR,Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees(
            [FromQuery] string? department,
            [FromQuery] string? jobTitle,
            [FromQuery] int? employeeID)
        {
            try
            {
                var employees = await _service.GetFilteredEmployeesAsync(department, jobTitle, employeeID);
                return Ok(employees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employees");
                return StatusCode(500, "Server error: Failed to retrieve employees");
            }
        }

        // GET: api/employees/5
        [Authorize(Roles = "HR,Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            try
            {
                var employee = await _service.GetEmployeeByIdAsync(id);
                return employee == null ? NotFound() : Ok(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching employee {Id}", id);
                return StatusCode(500, "Server error: Failed to retrieve employee");
            }
        }

        // GET: api/employees/headcount
        [Authorize(Roles = "HR,Admin")]
        [HttpGet("headcount")]
        public async Task<ActionResult<IEnumerable<EmployeeService.DepartmentHeadCount>>> GetDepartmentHeadCount()
        {
            try
            {
                var results = await _service.GetDepartmentHeadCountsAsync();
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching department headcount");
                return StatusCode(500, "Server error: Failed to retrieve department headcount");
            }
        }

        // GET: api/employees/search
        [Authorize(Roles = "HR,Admin")]
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Employee>>> SearchEmployees([FromQuery] string term)
        {
            try
            {
                var results = await _service.SearchEmployeesAsync(term);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search error for term: {Term}", term);
                return StatusCode(500, "Server error: Failed to perform search");
            }
        }
    }
}