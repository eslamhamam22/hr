using HrSystem.Application.Common.Interfaces;
using HrSystem.Application.Common.Models;
using HrSystem.Application.DTOs.ApprovalLog;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using HrSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Services.ApprovalLogs;

public class ApprovalLogService : IApprovalLogService
{
    private readonly IRepository<Domain.Entities.ApprovalLog> _approvalLogRepository;
    private readonly IRepository<User> _userRepository;

    public ApprovalLogService(
        IRepository<Domain.Entities.ApprovalLog> approvalLogRepository,
        IRepository<User> userRepository)
    {
        _approvalLogRepository = approvalLogRepository;
        _userRepository = userRepository;
    }

    public async Task<PaginatedResult<ApprovalLogDto>> GetApprovalLogsAsync(
        int page,
        int pageSize,
        RequestType? requestType,
        Guid? approvedByUserId,
        bool? approved,
        CancellationToken cancellationToken = default)
    {
        var logs = _approvalLogRepository.GetQueryable();

        if (requestType.HasValue)
        {
            logs = logs.Where(a => a.RequestType == requestType.Value);
        }

        if (approvedByUserId.HasValue)
        {
            logs = logs.Where(a => a.ApprovedByUserId == approvedByUserId.Value);
        }

        if (approved.HasValue)
        {
            logs = logs.Where(a => a.Approved == approved.Value);
        }

        var totalCount = await logs.CountAsync(cancellationToken);

        var items = await logs
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Fetch user names
        var userIds = items.Select(i => i.ApprovedByUserId).Distinct().ToList();
        var users = await _userRepository.GetQueryable()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);
            
        var dtos = items.Select(a => MapToDto(a, users)).ToList();

        return new PaginatedResult<ApprovalLogDto>(dtos, totalCount, page, pageSize);
    }

    public async Task<ApprovalLogDto?> GetApprovalLogByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var approvalLog = await _approvalLogRepository.GetByIdAsync(id, cancellationToken);

        if (approvalLog == null)
            return null;

        var user = await _userRepository.GetByIdAsync(approvalLog.ApprovedByUserId, cancellationToken);
        var users = new Dictionary<Guid, string>();
        if (user != null) users.Add(user.Id, user.FullName);

        return MapToDto(approvalLog, users);
    }

    public async Task<IEnumerable<ApprovalLogDto>> GetApprovalLogsForRequestAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        var logs = await _approvalLogRepository.GetQueryable()
            .Where(a => a.RequestId == requestId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        // Fetch user names
        var userIds = logs.Select(i => i.ApprovedByUserId).Distinct().ToList();
        var users = await _userRepository.GetQueryable()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);

        return logs.Select(a => MapToDto(a, users)).ToList();
    }

    public async Task LogApprovalAsync(
        Guid requestId,
        RequestType requestType,
        Guid approvedByUserId,
        bool approved,
        string? comments,
        CancellationToken cancellationToken = default)
    {
        var log = new Domain.Entities.ApprovalLog
        {
            RequestId = requestId,
            RequestType = requestType,
            ApprovedByUserId = approvedByUserId,
            Approved = approved,
            Comments = comments,
            IsOverride = false, // Defaulting to false for now
            CreatedAt = DateTime.UtcNow
        };

        await _approvalLogRepository.AddAsync(log, cancellationToken);
        await _approvalLogRepository.SaveChangesAsync(cancellationToken);
    }

    private ApprovalLogDto MapToDto(Domain.Entities.ApprovalLog a, Dictionary<Guid, string> users)
    {
        return new ApprovalLogDto
        {
            Id = a.Id,
            RequestId = a.RequestId,
            RequestType = a.RequestType.ToString(),
            ApprovedByUserId = a.ApprovedByUserId,
            ApprovedByUserName = users.ContainsKey(a.ApprovedByUserId) ? users[a.ApprovedByUserId] : "Unknown",
            Approved = a.Approved,
            Comments = a.Comments,
            IsOverride = a.IsOverride,
            OverrideReason = a.OverrideReason,
            CreatedAt = a.CreatedAt
        };
    }
}
