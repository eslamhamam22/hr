using HrSystem.Application.Common.Models;
using HrSystem.Application.DTOs.Requests;
using HrSystem.Domain.Enums;

namespace HrSystem.Application.Services.Requests;

/// <summary>
/// Application service for leave request use cases
/// </summary>
public interface ILeaveRequestService
{
    Task<Guid?> CreateLeaveRequestAsync(
        Guid userId, 
        int leaveTypeId, 
        DateTime startDate, 
        DateTime endDate, 
        string reason, 
        CancellationToken cancellationToken = default);
    
    Task<bool> SubmitLeaveRequestAsync(Guid requestId, CancellationToken cancellationToken = default);
    
    Task<bool> ApproveLeaveRequestAsync(Guid requestId, Guid approverUserId, CancellationToken cancellationToken = default);
    
    Task<bool> RejectLeaveRequestAsync(Guid requestId, Guid approverUserId, string rejectionReason, CancellationToken cancellationToken = default);

    Task<PaginatedResult<LeaveRequestDto>> GetLeaveRequestsAsync(
        int page,
        int pageSize,
        RequestStatus? status,
        Guid? userId = null,
        IEnumerable<Guid>? userIds = null,
        CancellationToken cancellationToken = default);

    Task<LeaveRequestDto?> GetLeaveRequestByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending leave requests for a manager to approve (their employees' requests with Submitted status)
    /// </summary>
    Task<IEnumerable<LeaveRequestDto>> GetPendingForManagerAsync(Guid managerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending leave requests for HR to approve (all requests with PendingHR or Submitted status)
    /// </summary>
    Task<IEnumerable<LeaveRequestDto>> GetPendingForHRAsync(CancellationToken cancellationToken = default);
}
