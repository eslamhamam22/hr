using HrSystem.Application.Common.Interfaces;
using HrSystem.Application.DTOs.WorkFromHome;
using HrSystem.Application.Services.WorkFromHome;
using HrSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.Api.Controllers;

/// <summary>
/// Work From Home requests controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkFromHomeController : ControllerBase
{
    private readonly IWorkFromHomeService _wfhService;
    private readonly ICurrentUserService _currentUserService;

    public WorkFromHomeController(
        IWorkFromHomeService wfhService,
        ICurrentUserService currentUserService)
    {
        _wfhService = wfhService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get all WFH requests with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWorkFromHomeRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] RequestStatus? status = null,
        [FromQuery] Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _wfhService.GetWorkFromHomeRequestsAsync(page, pageSize, status, userId, null, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific WFH request by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorkFromHomeRequest(Guid id, CancellationToken cancellationToken)
    {
        var request = await _wfhService.GetWorkFromHomeRequestByIdAsync(id, cancellationToken);
        if (request == null)
            return NotFound();

        return Ok(request);
    }

    /// <summary>
    /// Create a new WFH request
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateWorkFromHomeRequest(
        [FromBody] CreateWorkFromHomeRequestDto dto,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        if (userId == Guid.Empty)
            return Unauthorized();

        var result = await _wfhService.CreateWorkFromHomeRequestAsync(userId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetWorkFromHomeRequest), new { id = result.Id }, result);
    }

    /// <summary>
    /// Submit a draft WFH request
    /// </summary>
    [HttpPost("{id}/submit")]
    public async Task<IActionResult> SubmitWorkFromHomeRequest(Guid id, CancellationToken cancellationToken)
    {
        var success = await _wfhService.SubmitWorkFromHomeRequestAsync(id, cancellationToken);
        if (!success)
            return BadRequest(new { message = "Failed to submit request. It may not exist or is not in Draft status." });

        return Ok(new { message = "Work from home request submitted successfully" });
    }

    /// <summary>
    /// Approve a WFH request
    /// </summary>
    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Manager,HR,Admin")]
    public async Task<IActionResult> ApproveWorkFromHomeRequest(Guid id, CancellationToken cancellationToken)
    {
        var approverId = _currentUserService.UserId;
        if (approverId == Guid.Empty)
            return Unauthorized();

        var success = await _wfhService.ApproveWorkFromHomeRequestAsync(id, approverId, cancellationToken);
        if (!success)
            return BadRequest(new { message = "Failed to approve request." });

        return Ok(new { message = "Work from home request approved successfully" });
    }

    /// <summary>
    /// Reject a WFH request
    /// </summary>
    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Manager,HR,Admin")]
    public async Task<IActionResult> RejectWorkFromHomeRequest(
        Guid id,
        [FromBody] RejectRequestModel model,
        CancellationToken cancellationToken)
    {
        var approverId = _currentUserService.UserId;
        if (approverId == Guid.Empty)
            return Unauthorized();

        var success = await _wfhService.RejectWorkFromHomeRequestAsync(id, approverId, model.Reason, cancellationToken);
        if (!success)
            return BadRequest(new { message = "Failed to reject request." });

        return Ok(new { message = "Work from home request rejected successfully" });
    }

    /// <summary>
    /// Delete a draft WFH request
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWorkFromHomeRequest(Guid id, CancellationToken cancellationToken)
    {
        var success = await _wfhService.DeleteWorkFromHomeRequestAsync(id, cancellationToken);
        if (!success)
            return BadRequest(new { message = "Failed to delete request. It may not exist or is not in Draft status." });

        return Ok(new { message = "Work from home request deleted successfully" });
    }
}

public class RejectRequestModel
{
    public string Reason { get; set; } = string.Empty;
}
