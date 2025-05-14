using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkforceAPI.Data;
using WorkforceAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WorkforceAPI.Services; 

namespace WorkforceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly EmployeeContext _context;
        private readonly ILogger<ProjectsController> _logger;
        private readonly EmployeeService _service; 

        public ProjectsController(
            EmployeeContext context,
            ILogger<ProjectsController> logger,
            EmployeeService service)
        {
            _context = context;
            _logger = logger;
            _service = service; 
        }
        [Route("~/api/Project-Employees")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            try
            {
                var projects = await _context.Projects
                    .Include(p => p.Employees)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("Successfully retrieved {Count} projects", projects.Count);
                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting projects");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "Failed to retrieve projects",
                    Error = ex.Message
                });
            }
        }





        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<Project>>> GetUpcomingProjects()
        {
            try
            {
                var projects = await _context.Projects
                    .Where(p => p.StartDate > DateTime.Now)
                    .Include(p => p.Employees)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} upcoming projects", projects.Count);
                return Ok(projects);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting upcoming projects");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "Failed to retrieve upcoming projects",
                    Error = ex.Message
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Project>> CreateProject([FromBody] ProjectDto projectDto)
        {
            try
            {
                // Get selected employees
                var employees = await _service.GetEmployeesByIdsAsync(projectDto.EmployeeIds);

                var project = new Project
                {
                    Name = projectDto.Name,
                    Status = projectDto.Status ?? "Pending",
                    RequiredSkills = string.Join(",", projectDto.RequiredSkills),
                    StartDate = projectDto.StartDate,
                    EndDate = projectDto.EndDate,
                    Employees = employees.Select(e => new ProjectEmployee
                    {
                        EmployeeId = e.EmployeeID,
                        EmployeeName = e.EmployeeName
                    }).ToList()
                };

                _context.Projects.Add(project);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating project");
                return StatusCode(500, "Error creating project");
            }
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignEmployee([FromBody] ProjectAssignmentDto assignment)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Employees)
                    .FirstOrDefaultAsync(p => p.Id == assignment.ProjectId);

                if (project == null)
                {
                    _logger.LogWarning("Project with ID {ProjectId} not found", assignment.ProjectId);
                    return NotFound(new { Message = "Project not found" });
                }

                // Check if employee is already assigned
                if (project.Employees.Any(e => e.EmployeeId == assignment.EmployeeId))
                {
                    _logger.LogWarning("Employee {EmployeeId} already assigned to project {ProjectId}",
                        assignment.EmployeeId, assignment.ProjectId);
                    return Conflict(new { Message = "Employee already assigned to this project" });
                }

                project.Employees.Add(new ProjectEmployee
                {
                    EmployeeId = assignment.EmployeeId,
                    EmployeeName = assignment.EmployeeName
                });

                await _context.SaveChangesAsync();
                _logger.LogInformation("Assigned employee {EmployeeId} to project {ProjectId}",
                    assignment.EmployeeId, assignment.ProjectId);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning employee to project");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "Failed to assign employee to project",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProject(int id)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.Employees)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (project == null)
                {
                    _logger.LogWarning("Project with ID {ProjectId} not found", id);
                    return NotFound(new { Message = "Project not found" });
                }

                _logger.LogInformation("Retrieved project with ID {ProjectId}", id);
                return Ok(project);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting project with ID {ProjectId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    Message = "Failed to get project",
                    Error = ex.Message
                });
            }
        }
    }
}
