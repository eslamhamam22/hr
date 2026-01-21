using HrSystem.Application.Common.Models;
using HrSystem.Application.DTOs.ApprovalLog;
using HrSystem.Domain.Enums;

namespace HrSystem.Application.Services.ApprovalLogs;

public interface IApprovalLogService
{
    Task<PaginatedResult<ApprovalLogDto>> GetApprovalLogsAsync(
        int page,
        int pageSize,
        RequestType? requestType,
        Guid? approvedByUserId,
        bool? approved,
        CancellationToken cancellationToken = default);

    Task<ApprovalLogDto?> GetApprovalLogByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IEnumerable<ApprovalLogDto>> GetApprovalLogsForRequestAsync(Guid requestId, CancellationToken cancellationToken = default);

    Task LogApprovalAsync(
        Guid requestId,
        RequestType requestType,
        Guid approvedByUserId,
        bool approved,
        string? comments,
        CancellationToken cancellationToken = default);
}
