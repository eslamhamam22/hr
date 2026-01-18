using HrSystem.Application.Common.Models;
using HrSystem.Application.DTOs.ApprovalLog;

namespace HrSystem.Application.Services.ApprovalLogs;

public interface IApprovalLogService
{
    Task<PaginatedResult<ApprovalLogDto>> GetApprovalLogsAsync(
        int page,
        int pageSize,
        string? requestType,
        Guid? approvedByUserId,
        bool? approved,
        CancellationToken cancellationToken = default);

    Task<ApprovalLogDto?> GetApprovalLogByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
