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
        [FromQuery] Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _overtimeService.GetOvertimeRequestsAsync(page, pageSize, status, search, userId, cancellationToken: cancellationToken);
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
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized(new { message = "User not authenticated" });

        var overtime = await _overtimeService.CreateOvertimeRequestAsync(userId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetOvertimeRequestById), new { id = overtime.Id }, overtime);
    }

    /// <summary>
    /// Update an overtime request (draft only)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOvertimeRequest(Guid id, [FromBody] UpdateOvertimeRequestDto dto, CancellationToken cancellationToken)
    {
        // Optional: Check if user owns the request or logic is inside service
        try 
        {
            var result = await _overtimeService.UpdateOvertimeRequestAsync(id, dto, cancellationToken);
            
            if (result == null)
                return NotFound(new { message = "Overtime request not found" });
                
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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
    /// Approve an overtime request
    /// </summary>
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Manager,HR,Admin")]
    public async Task<IActionResult> ApproveOvertimeRequest(Guid id, CancellationToken cancellationToken)
    {
        var approverId = GetCurrentUserId();
        if (approverId == Guid.Empty) return Unauthorized();

        var result = await _overtimeService.ApproveOvertimeRequestAsync(id, approverId, cancellationToken);

        if (!result)
            return BadRequest(new { message = "Failed to approve request." });

        return Ok(new { message = "Overtime request approved" });
    }

    /// <summary>
    /// Reject an overtime request
    /// </summary>
    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Manager,HR,Admin")]
    public async Task<IActionResult> RejectOvertimeRequest(Guid id, [FromBody] RejectOvertimeRequestDto dto, CancellationToken cancellationToken)
    {
        var approverId = GetCurrentUserId();
        if (approverId == Guid.Empty) return Unauthorized();

        var result = await _overtimeService.RejectOvertimeRequestAsync(id, approverId, dto.Reason, cancellationToken);

        if (!result)
            return BadRequest(new { message = "Failed to reject request." });

        return Ok(new { message = "Overtime request rejected" });
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

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("nameid")?.Value;
            
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Guid.Empty;
        }
        return userId;
    }
}
