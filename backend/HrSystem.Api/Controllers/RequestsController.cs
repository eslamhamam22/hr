using HrSystem.Application.Common.Interfaces;
using HrSystem.Application.DTOs.Requests;
using HrSystem.Application.Services.Overtime;
using HrSystem.Application.Services.Requests;
using HrSystem.Application.Services.Users;
using HrSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HrSystem.Api.Controllers;

/// <summary>
/// Leave and overtime requests controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RequestsController : ControllerBase
{
    private readonly ILeaveRequestService _leaveRequestService;
    private readonly IOvertimeService _overtimeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;

    public RequestsController(
        ILeaveRequestService leaveRequestService,
        IOvertimeService overtimeService,
        ICurrentUserService currentUserService,
        IUserService userService)
    {
        _leaveRequestService = leaveRequestService;
        _overtimeService = overtimeService;
        _currentUserService = currentUserService;
        _userService = userService;
    }

    /// <summary>
    /// Get request history for a user
    /// </summary>
    [HttpGet("history/{userId}")]
    public async Task<IActionResult> GetRequestHistory(Guid userId, CancellationToken cancellationToken)
    {
        // Get Leave Requests (fetch all pages - simplified for history view, ideally use pagination)
        var leaveResult = await _leaveRequestService.GetLeaveRequestsAsync(1, 1000, null, userId, cancellationToken);
        
        // Get Overtime Requests
        var overtimeResult = await _overtimeService.GetOvertimeRequestsAsync(1, 1000, null, null, userId, cancellationToken);

        var history = new List<RequestSummaryDto>();

        history.AddRange(leaveResult.Items.Select(l => new RequestSummaryDto
        {
            Id = l.Id,
            EmployeeName = l.UserName ?? "Unknown", // Assuming UserName is populated
            RequestType = "Leave - " + l.LeaveTypeName, // e.g. "Leave - Annual"
            Status = l.Status.ToString(),
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            SubmittedAt = l.SubmittedAt ?? l.CreatedAt,
            ApprovedByName = l.ApprovedByHRName,
            ApprovedAt = l.ApprovedAt
        }));

        history.AddRange(overtimeResult.Items.Select(o => new RequestSummaryDto
        {
            Id = o.Id,
            EmployeeName = o.UserName ?? "Unknown",
            RequestType = "Overtime",
            Status = o.Status.ToString(),
            StartDate = o.StartDateTime,
            EndDate = o.EndDateTime,
            SubmittedAt = o.SubmittedAt ?? o.CreatedAt,
            ApprovedByName = o.ApprovedByHRName,
            ApprovedAt = o.ApprovedAt
        }));

        // Sort by CreatedAt/SubmittedAt desc
        var sortedHistory = history.OrderByDescending(x => x.SubmittedAt).ToList();

        return Ok(sortedHistory);
    }

    /// <summary>
    /// Get pending approvals based on user role:
    /// - Manager: See requests from their employees (Submitted status)
    /// - HR/Admin: See all pending requests (PendingHR and Submitted status)
    /// - Employee: No pending approvals (empty list)
    /// </summary>
    [HttpGet("pending-approvals")]
    public async Task<IActionResult> GetPendingApprovals(CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == Guid.Empty)
            return Unauthorized();

        // Get the current user to determine their role
        var currentUser = await _userService.GetUserByIdAsync(currentUserId, cancellationToken);
        if (currentUser == null)
            return Unauthorized();

        var pending = new List<RequestSummaryDto>();

        IEnumerable<Application.DTOs.Requests.LeaveRequestDto> leaveRequests;
        IEnumerable<Application.DTOs.Overtime.OvertimeRequestDto> overtimeRequests;

        switch (currentUser.Role)
        {
            case RoleType.HR:
            case RoleType.Admin:
                // HR and Admin can see all pending requests (both PendingHR and Submitted)
                leaveRequests = await _leaveRequestService.GetPendingForHRAsync(cancellationToken);
                overtimeRequests = await _overtimeService.GetPendingForHRAsync(cancellationToken);
                break;

            case RoleType.Manager:
                // Managers can only see requests from their subordinates (Submitted status)
                leaveRequests = await _leaveRequestService.GetPendingForManagerAsync(currentUserId, cancellationToken);
                overtimeRequests = await _overtimeService.GetPendingForManagerAsync(currentUserId, cancellationToken);
                break;

            case RoleType.Employee:
            default:
                // Regular employees cannot approve requests
                return Ok(pending);
        }

        pending.AddRange(leaveRequests.Select(l => new RequestSummaryDto
        {
            Id = l.Id,
            EmployeeName = l.UserName ?? "Unknown",
            RequestType = "Leave - " + l.LeaveTypeName,
            Status = l.Status.ToString(),
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            SubmittedAt = l.SubmittedAt ?? l.CreatedAt,
            ApprovedByName = l.ApprovedByHRName,
            ApprovedAt = l.ApprovedAt
        }));

        pending.AddRange(overtimeRequests.Select(o => new RequestSummaryDto
        {
            Id = o.Id,
            EmployeeName = o.UserName ?? "Unknown",
            RequestType = "Overtime",
            Status = o.Status.ToString(),
            StartDate = o.StartDateTime,
            EndDate = o.EndDateTime,
            SubmittedAt = o.SubmittedAt ?? o.CreatedAt,
            ApprovedByName = o.ApprovedByHRName,
            ApprovedAt = o.ApprovedAt
        }));

        var sortedPending = pending.OrderByDescending(x => x.SubmittedAt).ToList();

        return Ok(sortedPending);
    }

    /// <summary>
    /// Create a new leave request
    /// </summary>
    [HttpPost("leave")]
    public async Task<IActionResult> CreateLeaveRequest(
        [FromBody] CreateLeaveRequestModel request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _leaveRequestService.CreateLeaveRequestAsync(
            userId,
            request.LeaveTypeId,
            request.StartDate,
            request.EndDate,
            request.Reason,
            cancellationToken);

        return result != Guid.Empty ? Ok(new { message = "Leave request created successfully", id = result }) : BadRequest(new { message = "Failed to create leave request" });
    }

    /// <summary>
    /// Submit a request (Leave or Overtime)
    /// </summary>
    [HttpPost("{id}/submit")]
    public async Task<IActionResult> SubmitRequest(Guid id, CancellationToken cancellationToken)
    {
        // Try Leave first
        var leaveResult = await _leaveRequestService.SubmitLeaveRequestAsync(id, cancellationToken);
        if (leaveResult) return Ok(new { message = "Leave request submitted" });

        // Try Overtime
        var overtimeResult = await _overtimeService.SubmitOvertimeRequestAsync(id, cancellationToken);
        if (overtimeResult) return Ok(new { message = "Overtime request submitted" });

        return BadRequest(new { message = "Failed to submit request. It may not exist or is not in Draft status." });
    }

    /// <summary>
    /// Approve a request (Leave or Overtime)
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveRequest(Guid id, CancellationToken cancellationToken)
    {
        var approverId = GetCurrentUserId();
        if (approverId == Guid.Empty) return Unauthorized();

        // Try Leave first
        var leaveResult = await _leaveRequestService.ApproveLeaveRequestAsync(id, approverId, cancellationToken);
        if (leaveResult) return Ok(new { message = "Leave request approved" });

        // Try Overtime
        var overtimeResult = await _overtimeService.ApproveOvertimeRequestAsync(id, approverId, cancellationToken);
        if (overtimeResult) return Ok(new { message = "Overtime request approved" });

        return BadRequest(new { message = "Failed to approve request. It may not exist or is not in Submitted status." });
    }

    /// <summary>
    /// Reject a request (Leave or Overtime)
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectRequest(Guid id, [FromBody] RejectRequestModel model, CancellationToken cancellationToken)
    {
        var approverId = GetCurrentUserId();
        if (approverId == Guid.Empty) return Unauthorized();

        // Try Leave first
        var leaveResult = await _leaveRequestService.RejectLeaveRequestAsync(id, approverId, model.Reason, cancellationToken);
        if (leaveResult) return Ok(new { message = "Leave request rejected" });

        // Try Overtime
        var overtimeResult = await _overtimeService.RejectOvertimeRequestAsync(id, approverId, model.Reason, cancellationToken);
        if (overtimeResult) return Ok(new { message = "Overtime request rejected" });

        return BadRequest(new { message = "Failed to reject request. It may not exist or is not in Submitted status." });
    }

    private Guid GetCurrentUserId()
    {
        // Try multiple claim types for user ID
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

public class CreateLeaveRequestModel
{
    public int LeaveTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class RejectRequestModel
{
    public string Reason { get; set; } = string.Empty;
}
