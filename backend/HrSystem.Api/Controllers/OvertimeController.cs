using HrSystem.Application.DTOs.Overtime;
using HrSystem.Application.Services.Overtime;
using HrSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HrSystem.Api.Controllers;

/// <summary>
/// Overtime requests controller
/// </summary>
[ApiController]
[Route("api/requests/[controller]")]
[Authorize]
public class OvertimeController : ControllerBase
{
    private readonly IOvertimeService _overtimeService;

    public OvertimeController(IOvertimeService overtimeService)
    {
        _overtimeService = overtimeService;
    }

    /// <summary>
    /// Get paginated list of overtime requests
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOvertimeRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] RequestStatus? status = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _overtimeService.GetOvertimeRequestsAsync(page, pageSize, status, search, cancellationToken: cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get overtime request by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOvertimeRequestById(Guid id, CancellationToken cancellationToken)
    {
        var overtime = await _overtimeService.GetOvertimeRequestByIdAsync(id, cancellationToken);

        if (overtime == null)
            return NotFound(new { message = "Overtime request not found" });

        return Ok(overtime);
    }

    /// <summary>
    /// Create a new overtime request
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOvertimeRequest(
        [FromBody] CreateOvertimeRequestDto dto,
        CancellationToken cancellationToken)
    {
        // Get current user ID from claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { message = "User not authenticated" });
        }

        var overtime = await _overtimeService.CreateOvertimeRequestAsync(userId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetOvertimeRequestById), new { id = overtime.Id }, overtime);
    }

    /// <summary>
    /// Submit an overtime request
    /// </summary>
    [HttpPost("{id}/submit")]
    public async Task<IActionResult> SubmitOvertimeRequest(Guid id, CancellationToken cancellationToken)
    {
        var result = await _overtimeService.SubmitOvertimeRequestAsync(id, cancellationToken);

        if (!result)
            return BadRequest(new { message = "Failed to submit overtime request. It may not exist or not be in draft status." });

        return Ok(new { message = "Overtime request submitted successfully" });
    }

    /// <summary>
    /// Delete an overtime request (draft only)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOvertimeRequest(Guid id, CancellationToken cancellationToken)
    {
        var result = await _overtimeService.DeleteOvertimeRequestAsync(id, cancellationToken);

        if (!result)
            return BadRequest(new { message = "Failed to delete overtime request. It may not exist or not be in draft status." });

        return NoContent();
    }
}
