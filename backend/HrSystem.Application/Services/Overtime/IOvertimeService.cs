using HrSystem.Application.Common.Models;
using HrSystem.Application.DTOs.Overtime;
using HrSystem.Domain.Enums;

namespace HrSystem.Application.Services.Overtime;

public interface IOvertimeService
{
    Task<PaginatedResult<OvertimeRequestDto>> GetOvertimeRequestsAsync(
        int page,
        int pageSize,
        RequestStatus? status,
        string? search,
        Guid? userId = null,
        IEnumerable<Guid>? userIds = null,
        CancellationToken cancellationToken = default);

    Task<OvertimeRequestDto?> GetOvertimeRequestByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OvertimeRequestDto> CreateOvertimeRequestAsync(Guid userId, CreateOvertimeRequestDto dto, CancellationToken cancellationToken = default);

    Task<OvertimeRequestDto?> UpdateOvertimeRequestAsync(Guid id, UpdateOvertimeRequestDto dto, CancellationToken cancellationToken = default);

    Task<bool> SubmitOvertimeRequestAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> DeleteOvertimeRequestAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ApproveOvertimeRequestAsync(Guid id, Guid approverId, CancellationToken cancellationToken = default);

    Task<bool> RejectOvertimeRequestAsync(Guid id, Guid approverId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending overtime requests for a manager to approve (their employees' requests with Submitted status)
    /// </summary>
    Task<IEnumerable<OvertimeRequestDto>> GetPendingForManagerAsync(Guid managerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending overtime requests for HR to approve (all requests with PendingHR or Submitted status)
    /// </summary>
    Task<IEnumerable<OvertimeRequestDto>> GetPendingForHRAsync(CancellationToken cancellationToken = default);
}
