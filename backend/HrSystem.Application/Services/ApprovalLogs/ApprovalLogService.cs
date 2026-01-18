using HrSystem.Application.Common.Interfaces;
using HrSystem.Application.Common.Models;
using HrSystem.Application.DTOs.ApprovalLog;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Services.ApprovalLogs;

public class ApprovalLogService : IApprovalLogService
{
    private readonly IRepository<Domain.Entities.ApprovalLog> _approvalLogRepository;

    public ApprovalLogService(IRepository<Domain.Entities.ApprovalLog> approvalLogRepository)
    {
        _approvalLogRepository = approvalLogRepository;
    }

    public async Task<PaginatedResult<ApprovalLogDto>> GetApprovalLogsAsync(
        int page,
        int pageSize,
        string? requestType,
        Guid? approvedByUserId,
        bool? approved,
        CancellationToken cancellationToken = default)
    {
        // Fetch all logs and filter in-memory (not optimal for large datasets)
        var allLogs = await _approvalLogRepository.GetAllAsync(cancellationToken);

        var filteredLogs = allLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(requestType))
        {
            filteredLogs = filteredLogs.Where(a => a.RequestType == requestType);
        }

        if (approvedByUserId.HasValue)
        {
            filteredLogs = filteredLogs.Where(a => a.ApprovedByUserId == approvedByUserId.Value);
        }

        if (approved.HasValue)
        {
            filteredLogs = filteredLogs.Where(a => a.Approved == approved.Value);
        }

        var totalCount = filteredLogs.Count();

        var items = filteredLogs
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ApprovalLogDto
            {
                Id = a.Id,
                RequestId = a.RequestId,
                RequestType = a.RequestType,
                ApprovedByUserId = a.ApprovedByUserId,
                ApprovedByUserName = null, // Can't include navigation property without IQueryable
                Approved = a.Approved,
                Comments = a.Comments,
                IsOverride = a.IsOverride,
                OverrideReason = a.OverrideReason,
                CreatedAt = a.CreatedAt
            })
            .ToList();

        return new PaginatedResult<ApprovalLogDto>(items, totalCount, page, pageSize);
    }

    public async Task<ApprovalLogDto?> GetApprovalLogByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var approvalLog = await _approvalLogRepository.GetByIdAsync(id, cancellationToken);

        if (approvalLog == null)
            return null;

        return new ApprovalLogDto
        {
            Id = approvalLog.Id,
            RequestId = approvalLog.RequestId,
            RequestType = approvalLog.RequestType,
            ApprovedByUserId = approvalLog.ApprovedByUserId,
            ApprovedByUserName = null, // Can't include navigation property without IQueryable
            Approved = approvalLog.Approved,
            Comments = approvalLog.Comments,
            IsOverride = approvalLog.IsOverride,
            OverrideReason = approvalLog.OverrideReason,
            CreatedAt = approvalLog.CreatedAt
        };
    }
}
