using HrSystem.Application.Common.Interfaces;
using HrSystem.Application.DTOs.Requests;
using HrSystem.Application.Services.Overtime;
using HrSystem.Application.Services.Requests;
using HrSystem.Application.Services.Users;
using HrSystem.Application.Services.WorkFromHome;
using HrSystem.Application.Services.TimeOff;
using HrSystem.Application.DTOs.TimeOff;
using HrSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HrSystem.Api.Controllers;

/// <summary>
/// Leave, overtime, and work from home requests controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RequestsController : ControllerBase
{
    private readonly ILeaveRequestService _leaveRequestService;
    private readonly IOvertimeService _overtimeService;
    private readonly IWorkFromHomeService _workFromHomeService;
    private readonly ITimeOffService _timeOffService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;

    public RequestsController(
        ILeaveRequestService leaveRequestService,
        IOvertimeService overtimeService,
        IWorkFromHomeService workFromHomeService,
        ITimeOffService timeOffService,
        ICurrentUserService currentUserService,
        IUserService userService)
    {
        _leaveRequestService = leaveRequestService;
        _overtimeService = overtimeService;
        _workFromHomeService = workFromHomeService;
        _timeOffService = timeOffService;
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
        var leaveResult = await _leaveRequestService.GetLeaveRequestsAsync(1, 1000, null, userId, null, cancellationToken);
        
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

        // Sort by CreatedAt/SubmittedAt desc
        var sortedHistory = history.OrderByDescending(x => x.SubmittedAt).ToList();

        return Ok(sortedHistory);
    }

    /// <summary>
    /// Get all requests with filtering options (for Manager/HR/Admin)
    /// Supports filtering by userId, departmentId, status, and date range
    /// </summary>
    [HttpGet("all")]
    public async Task<IActionResult> GetAllRequests(
        [FromQuery] Guid? userId,
        [FromQuery] Guid? departmentId,
        [FromQuery] RequestStatus? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == Guid.Empty)
            return Unauthorized();

        var currentUser = await _userService.GetUserByIdAsync(currentUserId, cancellationToken);
        if (currentUser == null)
            return Unauthorized();

        IEnumerable<Guid>? allowedUserIds = null;

        // Role-based filtering logic
        if (currentUser.Role == RoleType.Manager)
        {
            // Managers can see their own requests and their subordinates' requests
            var subordinates = await _userService.GetSubordinatesAsync(currentUserId, cancellationToken);
            var subordinateIds = subordinates.Select(u => u.Id).ToList();
            subordinateIds.Add(currentUserId); // Add self
            
            allowedUserIds = subordinateIds;

            // If a specific user is requested, ensure they are in the allowed list
            if (userId.HasValue)
            {
                if (!allowedUserIds.Contains(userId.Value))
                {
                    return Forbid();
                }
                // Allowed, rely on userId filter in service which intersects with userIds if we passed both,
                // but since we verified, we can just pass userId. 
                // However, to be safe and simple, we can pass allowedUserIds to the service OR handle it here.
                // Since userId is stricter, we can just pass userId effectively (as we verified it's allowed).
                // But for consistency let's rely on logic below.
            }
        }
        else if (currentUser.Role == RoleType.Employee)
        {
            // Employees can only see their own requests
            if (userId.HasValue && userId.Value != currentUserId)
            {
                return Forbid();
            }
            userId = currentUserId; // Force filter to self
        }
        else if (currentUser.Role == RoleType.HR || currentUser.Role == RoleType.Admin)
        {
            // HR/Admin can see all - no restrictions on allowedUserIds (stays null)
        }
        else
        {
             // Fallback for unknown roles - restrict to self
             userId = currentUserId;
        }

        // Get Leave Requests
        var leaveResult = await _leaveRequestService.GetLeaveRequestsAsync(1, 10000, status, userId, allowedUserIds, cancellationToken);
        
        // Get Overtime Requests
        var overtimeResult = await _overtimeService.GetOvertimeRequestsAsync(1, 10000, status, null, userId, allowedUserIds, cancellationToken);

        // Get Work From Home Requests
        var wfhResult = await _workFromHomeService.GetWorkFromHomeRequestsAsync(1, 10000, status, userId, allowedUserIds, cancellationToken);

        // Get Time Off Requests
        var timeOffResult = await _timeOffService.GetTimeOffRequestsAsync(1, 10000, status, userId, allowedUserIds, cancellationToken);

        var allRequests = new List<RequestSummaryDto>();

        // Map leave requests
        allRequests.AddRange(leaveResult.Items.Select(l => new RequestSummaryDto
        {
            Id = l.Id,
            EmployeeName = l.UserName ?? "Unknown",
            RequestType = "Leave - " + l.LeaveTypeName,
            Status = l.Status.ToString(),
            StartDate = l.StartDate,
            EndDate = l.EndDate,
            SubmittedAt = l.SubmittedAt ?? l.CreatedAt,
            ApprovedByName = l.ApprovedByHRName,
            ApprovedAt = l.ApprovedAt,
            UserId = l.UserId,
            DepartmentId = null // Will be populated from user if needed
        }));

        // Map overtime requests
        allRequests.AddRange(overtimeResult.Items.Select(o => new RequestSummaryDto
        {
            Id = o.Id,
            EmployeeName = o.UserName ?? "Unknown",
            RequestType = "Overtime",
            Status = o.Status.ToString(),
            StartDate = o.StartDateTime,
            EndDate = o.EndDateTime,
            SubmittedAt = o.SubmittedAt ?? o.CreatedAt,
            ApprovedByName = o.ApprovedByHRName,
            ApprovedAt = o.ApprovedAt,
            UserId = o.UserId,
            DepartmentId = null // Will be populated from user if needed
        }));

        // Map work from home requests
        allRequests.AddRange(wfhResult.Items.Select(w => new RequestSummaryDto
        {
            Id = w.Id,
            EmployeeName = w.UserName ?? "Unknown",
            RequestType = "Work From Home",
            Status = w.Status.ToString(),
            StartDate = w.FromDate,
            EndDate = w.ToDate,
            SubmittedAt = w.SubmittedAt ?? w.CreatedAt,
            ApprovedByName = w.ApprovedByHRName,
            ApprovedAt = w.ApprovedAt,
            UserId = w.UserId,
            DepartmentId = null // Will be populated from user if needed
        }));

        // Map time off requests
        allRequests.AddRange(timeOffResult.Items.Select(t => new RequestSummaryDto
        {
            Id = t.Id,
            EmployeeName = t.UserName ?? "Unknown",
            RequestType = "Time Off",
            Status = t.Status.ToString(),
            StartDate = t.Date.Add(t.StartTime), // Combine Date and StartTime
            EndDate = t.Date.Add(t.StartTime).AddHours(2), // 2 hours duration
            SubmittedAt = t.SubmittedAt ?? t.CreatedAt,
            ApprovedByName = t.ApprovedByHRName,
            ApprovedAt = t.ApprovedAt,
            UserId = t.UserId,
            DepartmentId = null
        }));

        // Apply date range filter
        if (fromDate.HasValue)
        {
            allRequests = allRequests.Where(r => r.StartDate >= fromDate.Value).ToList();
        }
        if (toDate.HasValue)
        {
            allRequests = allRequests.Where(r => r.EndDate <= toDate.Value).ToList();
        }

        // Apply department filter if specified
        if (departmentId.HasValue)
        {
            var userIds = allRequests.Select(r => r.UserId).Distinct().ToList();
            var usersInDept = await _userService.GetUsersByDepartmentAsync(departmentId.Value, cancellationToken);
            var deptUserIds = usersInDept.Select(u => u.Id).ToHashSet();
            allRequests = allRequests.Where(r => r.UserId.HasValue && deptUserIds.Contains(r.UserId.Value)).ToList();
        }

        // Sort by submitted date descending
        var sortedRequests = allRequests.OrderByDescending(x => x.SubmittedAt).ToList();

        return Ok(sortedRequests);
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
        IEnumerable<Application.DTOs.WorkFromHome.WorkFromHomeRequestDto> wfhRequests;
        IEnumerable<Application.DTOs.TimeOff.TimeOffRequestDto> timeOffRequests;

        switch (currentUser.Role)
        {
            case RoleType.HR:
            case RoleType.Admin:
                // HR and Admin can see all pending requests (both PendingHR and Submitted)
                leaveRequests = await _leaveRequestService.GetPendingForHRAsync(cancellationToken);
                overtimeRequests = await _overtimeService.GetPendingForHRAsync(cancellationToken);
                wfhRequests = await _workFromHomeService.GetPendingForHRAsync(cancellationToken);
                timeOffRequests = await _timeOffService.GetPendingForHRAsync(cancellationToken);
                break;

            case RoleType.Manager:
                // Managers can only see requests from their subordinates (Submitted status)
                leaveRequests = await _leaveRequestService.GetPendingForManagerAsync(currentUserId, cancellationToken);
                overtimeRequests = await _overtimeService.GetPendingForManagerAsync(currentUserId, cancellationToken);
                wfhRequests = await _workFromHomeService.GetPendingForManagerAsync(currentUserId, cancellationToken);
                timeOffRequests = await _timeOffService.GetPendingForManagerAsync(currentUserId, cancellationToken);
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

        pending.AddRange(wfhRequests.Select(w => new RequestSummaryDto
        {
            Id = w.Id,
            EmployeeName = w.UserName ?? "Unknown",
            RequestType = "Work From Home",
            Status = w.Status.ToString(),
            StartDate = w.FromDate,
            EndDate = w.ToDate,
            SubmittedAt = w.SubmittedAt ?? w.CreatedAt,
            ApprovedByName = w.ApprovedByHRName,
            ApprovedAt = w.ApprovedAt
        }));

        pending.AddRange(timeOffRequests.Select(t => new RequestSummaryDto
        {
            Id = t.Id,
            EmployeeName = t.UserName ?? "Unknown",
            RequestType = "Time Off",
            Status = t.Status.ToString(),
            StartDate = t.Date.Add(t.StartTime),
            EndDate = t.Date.Add(t.StartTime).AddHours(2),
            SubmittedAt = t.SubmittedAt ?? t.CreatedAt,
            ApprovedByName = t.ApprovedByHRName,
            ApprovedAt = t.ApprovedAt
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
