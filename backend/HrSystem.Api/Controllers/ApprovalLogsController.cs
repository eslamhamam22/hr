using HrSystem.Application.Services.ApprovalLogs;
using HrSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.Api.Controllers;

/// <summary>
/// Approval logs controller
/// </summary>
[ApiController]
[Route("api/approval-logs")]
[Authorize]
public class ApprovalLogsController : ControllerBase
{
    private readonly IApprovalLogService _approvalLogService;

    public ApprovalLogsController(IApprovalLogService approvalLogService)
    {
        _approvalLogService = approvalLogService;
    }

    /// <summary>
    /// Get paginated list of approval logs
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetApprovalLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? requestType = null,
        [FromQuery] Guid? approvedByUserId = null,
        [FromQuery] bool? approved = null,
        CancellationToken cancellationToken = default)
    {
        RequestType? typeEnum = null;
        if (!string.IsNullOrEmpty(requestType) && Enum.TryParse<RequestType>(requestType, true, out var parsed))
        {
            typeEnum = parsed;
        }

        var result = await _approvalLogService.GetApprovalLogsAsync(page, pageSize, typeEnum, approvedByUserId, approved, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get approval log by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetApprovalLogById(Guid id, CancellationToken cancellationToken)
    {
        var approvalLog = await _approvalLogService.GetApprovalLogByIdAsync(id, cancellationToken);

        if (approvalLog == null)
            return NotFound(new { message = "Approval log not found" });

        return Ok(approvalLog);
    }

    /// <summary>
    /// Get all approval logs for a specific request
    /// </summary>
    [HttpGet("request/{requestId}")]
    public async Task<IActionResult> GetApprovalLogsForRequest(Guid requestId, CancellationToken cancellationToken)
    {
        var logs = await _approvalLogService.GetApprovalLogsForRequestAsync(requestId, cancellationToken);
        return Ok(logs);
    }
}
