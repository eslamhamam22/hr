using HrSystem.Application.DTOs.Requests;
using HrSystem.Application.Services.Overtime;
using HrSystem.Application.Services.Requests;
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

    public RequestsController(
        ILeaveRequestService leaveRequestService,
        IOvertimeService overtimeService)
    {
        _leaveRequestService = leaveRequestService;
        _overtimeService = overtimeService;
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
    /// Get pending approvals
    /// </summary>
    [HttpGet("pending-approvals")]
    public async Task<IActionResult> GetPendingApprovals(CancellationToken cancellationToken)
    {
        // Get Pending Leave Requests
        var leaveResult = await _leaveRequestService.GetLeaveRequestsAsync(1, 1000, RequestStatus.Submitted, null, cancellationToken);
        
        // Get Pending Overtime Requests
        var overtimeResult = await _overtimeService.GetOvertimeRequestsAsync(1, 1000, RequestStatus.Submitted, null, null, cancellationToken);

        var pending = new List<RequestSummaryDto>();

        pending.AddRange(leaveResult.Items.Select(l => new RequestSummaryDto
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

        pending.AddRange(overtimeResult.Items.Select(o => new RequestSummaryDto
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

        return result ? Ok("Leave request created successfully") : BadRequest("Failed to create leave request");
    }

    /// <summary>
    /// Submit a request (Leave or Overtime)
    /// </summary>
    [HttpPost("{id}/submit")]
    public async Task<IActionResult> SubmitRequest(Guid id, CancellationToken cancellationToken)
    {
        // Try Leave first
        var leaveResult = await _leaveRequestService.SubmitLeaveRequestAsync(id, cancellationToken);
        if (leaveResult) return Ok("Leave request submitted");

        // Try Overtime
        var overtimeResult = await _overtimeService.SubmitOvertimeRequestAsync(id, cancellationToken);
        if (overtimeResult) return Ok("Overtime request submitted");

        return BadRequest("Failed to submit request. It may not exist or is not in Draft status.");
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

        return BadRequest("Failed to approve request. It may not exist or is not in Submitted status.");
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

        return BadRequest("Failed to reject request. It may not exist or is not in Submitted status.");
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
