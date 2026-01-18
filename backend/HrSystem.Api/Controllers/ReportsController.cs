using HrSystem.Application.Services.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.Api.Controllers;

/// <summary>
/// Reports controller for HR analytics
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireHRRole")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>
    /// Get attendance report
    /// </summary>
    [HttpGet("attendance")]
    public async Task<IActionResult> GetAttendanceReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] Guid? employeeId = null,
        CancellationToken cancellationToken = default)
    {
        var report = await _reportService.GetAttendanceReportAsync(
            startDate,
            endDate,
            departmentId,
            employeeId,
            cancellationToken);

        return Ok(report);
    }

    /// <summary>
    /// Get leave summary report
    /// </summary>
    [HttpGet("leave-summary")]
    public async Task<IActionResult> GetLeaveSummaryReport(
        [FromQuery] Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var report = await _reportService.GetLeaveSummaryReportAsync(departmentId, cancellationToken);
        return Ok(report);
    }

    /// <summary>
    /// Get overtime audit report
    /// </summary>
    [HttpGet("overtime-audit")]
    public async Task<IActionResult> GetOvertimeAuditReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var report = await _reportService.GetOvertimeAuditReportAsync(
            startDate,
            endDate,
            departmentId,
            cancellationToken);

        return Ok(report);
    }
}
