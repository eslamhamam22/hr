using HrSystem.Application.Services.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    public RequestsController(ILeaveRequestService leaveRequestService)
    {
        _leaveRequestService = leaveRequestService;
    }

    /// <summary>
    /// Create a new leave request
    /// </summary>
    [HttpPost("leave")]
    public async Task<IActionResult> CreateLeaveRequest(
        [FromBody] CreateLeaveRequestModel request,
        CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.CreateLeaveRequestAsync(
            Guid.NewGuid(), // Current user ID would come from claims
            request.LeaveTypeId,
            request.StartDate,
            request.EndDate,
            request.Reason,
            cancellationToken);

        return result ? Ok("Leave request created successfully") : BadRequest("Failed to create leave request");
    }

    /// <summary>
    /// Submit a draft leave request
    /// </summary>
    [HttpPost("{id}/submit")]
    public async Task<IActionResult> SubmitLeaveRequest(Guid id, CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.SubmitLeaveRequestAsync(id, cancellationToken);
        return result ? Ok("Leave request submitted") : BadRequest("Failed to submit leave request");
    }
}

public class CreateLeaveRequestModel
{
    public int LeaveTypeId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}
