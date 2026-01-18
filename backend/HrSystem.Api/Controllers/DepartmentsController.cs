using HrSystem.Application.DTOs.Department;
using HrSystem.Application.Services.Departments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.Api.Controllers;

/// <summary>
/// Department management controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    /// <summary>
    /// Get paginated list of departments
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetDepartments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _departmentService.GetDepartmentsAsync(page, pageSize, search, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get department by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDepartmentById(Guid id, CancellationToken cancellationToken)
    {
        var department = await _departmentService.GetDepartmentByIdAsync(id, cancellationToken);

        if (department == null)
            return NotFound(new { message = "Department not found" });

        return Ok(department);
    }

    /// <summary>
    /// Create a new department
    /// </summary>
    [HttpPost]
    //[Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> CreateDepartment(
        [FromBody] CreateDepartmentDto dto,
        CancellationToken cancellationToken)
    {
        var department = await _departmentService.CreateDepartmentAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetDepartmentById), new { id = department.Id }, department);
    }

    /// <summary>
    /// Update an existing department
    /// </summary>
    [HttpPut("{id}")]
    //[Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> UpdateDepartment(
        Guid id,
        [FromBody] UpdateDepartmentDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _departmentService.UpdateDepartmentAsync(id, dto, cancellationToken);

        if (!result)
            return NotFound(new { message = "Department not found" });

        return NoContent();
    }

    /// <summary>
    /// Delete a department
    /// </summary>
    [HttpDelete("{id}")]
    //[Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> DeleteDepartment(Guid id, CancellationToken cancellationToken)
    {
        var result = await _departmentService.DeleteDepartmentAsync(id, cancellationToken);

        if (!result)
            return NotFound(new { message = "Department not found" });

        return NoContent();
    }
}
