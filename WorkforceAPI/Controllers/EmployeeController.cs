using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WorkforceAPI.Models;
using WorkforceAPI.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkforceAPI.Controllers
{
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

        
        [HttpGet]
        public async Task<ActionResult<List<Employee>>> GetEmployees()
        {
            try
            {
                var employees = await _service.GetAllEmployeesAsync();
                return Ok(employees);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all employees");
                return StatusCode(500, "Server error: Failed to retrieve employees");
            }
        }

        [HttpGet("headcount")]
        public async Task<ActionResult<List<EmployeeService.DepartmentHeadCount>>> GetDepartmentHeadCount()
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

        /
        [HttpGet("search")]
        public async Task<ActionResult<List<Employee>>> SearchEmployees([FromQuery] string term)
        {
            try
            {
                var results = await _service.SearchEmployeesAsync(term);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Search error for term: {term}");
                return StatusCode(500, "Server error: Failed to perform search");
            }
        }
    }
}
