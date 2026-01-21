using HrSystem.Application.Common.Interfaces;
using HrSystem.Application.DTOs.Requests;
using HrSystem.Application.DTOs.TimeOff;
using HrSystem.Application.Services.TimeOff;
using HrSystem.Application.Services.Users;
using HrSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HrSystem.Api.Controllers;

/// <summary>
/// Time off requests controller
/// </summary>
[ApiController]
[Route("api/time-off-requests")]
[Authorize]
public class TimeOffRequestsController : ControllerBase
{
    private readonly ITimeOffService _timeOffService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;

    public TimeOffRequestsController(
        ITimeOffService timeOffService,
        ICurrentUserService currentUserService,
        IUserService userService)
    {
        _timeOffService = timeOffService;
        _currentUserService = currentUserService;
        _userService = userService;
    }

    /// <summary>
    /// Get time off requests (filtered)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTimeOffRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] RequestStatus? status = null,
        [FromQuery] Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == Guid.Empty) return Unauthorized();

        var currentUser = await _userService.GetUserByIdAsync(currentUserId, cancellationToken);
        if (currentUser == null) return Unauthorized();

        IEnumerable<Guid>? allowedUserIds = null;

        // Role-based filtering logic similar to RequestsController
        if (currentUser.Role == RoleType.Manager)
        {
            var subordinates = await _userService.GetSubordinatesAsync(currentUserId, cancellationToken);
            var subordinateIds = subordinates.Select(u => u.Id).ToList();
            subordinateIds.Add(currentUserId);
            allowedUserIds = subordinateIds;

            if (userId.HasValue && !allowedUserIds.Contains(userId.Value))
            {
                return Forbid();
            }
        }
        else if (currentUser.Role == RoleType.Employee)
        {
            if (userId.HasValue && userId.Value != currentUserId) return Forbid();
            userId = currentUserId; // Restrict to self
        }
        
        var result = await _timeOffService.GetTimeOffRequestsAsync(page, pageSize, status, userId, allowedUserIds, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get time off request by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTimeOffRequestById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _timeOffService.GetTimeOffRequestByIdAsync(id, cancellationToken);
        if (result == null) return NotFound();
        
        // Add authorization check here if needed (e.g. only own or manager/HR)
        
        return Ok(result);
    }

    /// <summary>
    /// Create a new time off request
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTimeOffRequest(
        [FromBody] CreateTimeOffRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        try 
        {
            var result = await _timeOffService.CreateTimeOffRequestAsync(
                userId,
                request,
                cancellationToken);
            return Ok(result);
            // return Ok(new { message = "Time off request created successfully", id = result.Id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Submit a time off request
    /// </summary>
    [HttpPost("{id}/submit")]
    public async Task<IActionResult> SubmitTimeOffRequest(Guid id, CancellationToken cancellationToken)
    {
        var result = await _timeOffService.SubmitTimeOffRequestAsync(id, cancellationToken);
        if (result) return Ok(new { message = "Time off request submitted" });
        return BadRequest(new { message = "Failed to submit request. It may not exist or is not in Draft status." });
    }

    /// <summary>
    /// Approve a time off request
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveTimeOffRequest(Guid id, CancellationToken cancellationToken)
    {
        var approverId = GetCurrentUserId();
        if (approverId == Guid.Empty) return Unauthorized();

        var result = await _timeOffService.ApproveTimeOffRequestAsync(id, approverId, cancellationToken);
        if (result) return Ok(new { message = "Time off request approved" });
        return BadRequest(new { message = "Failed to approve request." });
    }

    /// <summary>
    /// Reject a time off request
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectTimeOffRequest(Guid id, [FromBody] RejectRequestModel model, CancellationToken cancellationToken)
    {
        var approverId = GetCurrentUserId();
        if (approverId == Guid.Empty) return Unauthorized();

        var result = await _timeOffService.RejectTimeOffRequestAsync(id, approverId, model.Reason, cancellationToken);
        if (result) return Ok(new { message = "Time off request rejected" });
        return BadRequest(new { message = "Failed to reject request." });
    }

    /// <summary>
    /// Delete a time off request
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTimeOffRequest(Guid id, CancellationToken cancellationToken)
    {
        var result = await _timeOffService.DeleteTimeOffRequestAsync(id, cancellationToken);
        if (result) return Ok(new { message = "Time off request deleted" });
        return BadRequest(new { message = "Failed to delete request. It may not exist or is not in Draft status." });
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
