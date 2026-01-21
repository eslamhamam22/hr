using HrSystem.Application.Common.Models;
using HrSystem.Application.DTOs.WorkFromHome;
using HrSystem.Domain.Enums;

namespace HrSystem.Application.Services.WorkFromHome;

public interface IWorkFromHomeService
{
    Task<PaginatedResult<WorkFromHomeRequestDto>> GetWorkFromHomeRequestsAsync(
        int page,
        int pageSize,
        RequestStatus? status,
        Guid? userId = null,
        IEnumerable<Guid>? userIds = null,
        CancellationToken cancellationToken = default);

    Task<WorkFromHomeRequestDto?> GetWorkFromHomeRequestByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<WorkFromHomeRequestDto> CreateWorkFromHomeRequestAsync(
        Guid userId, 
        CreateWorkFromHomeRequestDto dto, 
        CancellationToken cancellationToken = default);

    Task<bool> SubmitWorkFromHomeRequestAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> DeleteWorkFromHomeRequestAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ApproveWorkFromHomeRequestAsync(Guid id, Guid approverId, CancellationToken cancellationToken = default);

    Task<bool> RejectWorkFromHomeRequestAsync(Guid id, Guid approverId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending WFH requests for a manager to approve (their employees' requests with Submitted status)
    /// </summary>
    Task<IEnumerable<WorkFromHomeRequestDto>> GetPendingForManagerAsync(Guid managerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending WFH requests for HR to approve (all requests with PendingHR or Submitted status)
    /// </summary>
    Task<IEnumerable<WorkFromHomeRequestDto>> GetPendingForHRAsync(CancellationToken cancellationToken = default);
}
