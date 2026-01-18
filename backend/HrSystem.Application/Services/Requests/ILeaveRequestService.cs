namespace HrSystem.Application.Services.Requests;

/// <summary>
/// Application service for leave request use cases
/// </summary>
public interface ILeaveRequestService
{
    Task<bool> CreateLeaveRequestAsync(
        Guid userId, 
        int leaveTypeId, 
        DateTime startDate, 
        DateTime endDate, 
        string reason, 
        CancellationToken cancellationToken = default);
    
    Task<bool> SubmitLeaveRequestAsync(Guid requestId, CancellationToken cancellationToken = default);
    
    Task<bool> ApproveLeaveRequestAsync(Guid requestId, Guid approverUserId, CancellationToken cancellationToken = default);
    
    Task<bool> RejectLeaveRequestAsync(Guid requestId, Guid approverUserId, string rejectionReason, CancellationToken cancellationToken = default);
}
