using HrSystem.Application.Services.Dashboard;
using HrSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HrSystem.Api.Controllers;

/// <summary>
/// Dashboard controller for employee and manager dashboards
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// Get employee dashboard data
    /// </summary>
    [HttpGet("employee")]
    public async Task<IActionResult> GetEmployeeDashboard(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var dashboard = await _dashboardService.GetEmployeeDashboardAsync(userId, cancellationToken);
        return Ok(dashboard);
    }

    /// <summary>
    /// Get manager dashboard data (includes team statistics)
    /// </summary>
    [HttpGet("manager")]
    public async Task<IActionResult> GetManagerDashboard(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var dashboard = await _dashboardService.GetManagerDashboardAsync(userId, cancellationToken);
        return Ok(dashboard);
    }

    /// <summary>
    /// Get admin/HR dashboard data (includes system-wide statistics)
    /// </summary>
    [HttpGet("admin")]
    [Authorize(Policy = "RequireHRRole")]
    public async Task<IActionResult> GetAdminDashboard(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var dashboard = await _dashboardService.GetAdminDashboardAsync(userId, cancellationToken);
        return Ok(dashboard);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Guid.Empty;
        }
        return userId;
    }
}
