using HrSystem.Application.Services.Attendance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.Api.Controllers;

/// <summary>
/// Attendance logs controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    /// <summary>
    /// Get paginated list of attendance logs
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAttendanceLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? userId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _attendanceService.GetAttendanceLogsAsync(page, pageSize, userId, startDate, endDate, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get attendance log by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAttendanceLogById(Guid id, CancellationToken cancellationToken)
    {
        var attendanceLog = await _attendanceService.GetAttendanceLogByIdAsync(id, cancellationToken);

        if (attendanceLog == null)
            return NotFound(new { message = "Attendance log not found" });

        return Ok(attendanceLog);
    }
}
