using HrSystem.Application.Common.Models;
using HrSystem.Application.DTOs.TimeOff;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using HrSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using HrSystem.Application.Services.ApprovalLogs;

namespace HrSystem.Application.Services.TimeOff;

public class TimeOffService : ITimeOffService
{
    private readonly IRepository<TimeOffRequest> _timeOffRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IApprovalLogService _approvalLogService;

    public TimeOffService(
        IRepository<TimeOffRequest> timeOffRepository,
        IRepository<User> userRepository,
        IApprovalLogService approvalLogService)
    {
        _timeOffRepository = timeOffRepository;
        _userRepository = userRepository;
        _approvalLogService = approvalLogService;
    }

    public async Task<PaginatedResult<TimeOffRequestDto>> GetTimeOffRequestsAsync(
        int page,
        int pageSize,
        RequestStatus? status,
        Guid? userId = null,
        IEnumerable<Guid>? userIds = null,
        CancellationToken cancellationToken = default)
    {
        var query = _timeOffRepository.GetQueryable();

        if (userId.HasValue)
        {
            query = query.Where(r => r.UserId == userId.Value);
        }

        if (userIds != null && userIds.Any())
        {
            query = query.Where(r => userIds.Contains(r.UserId));
        }

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var requests = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var userIdsReq = requests.Select(r => r.UserId).Distinct().ToList();
        var users = await _userRepository.GetQueryable()
            .Where(u => userIdsReq.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync(cancellationToken);
        var userNames = users.ToDictionary(u => u.Id, u => u.FullName);

        var items = requests.Select(r => MapToDto(r, userNames.TryGetValue(r.UserId, out var name) ? name : null)).ToList();

        return new PaginatedResult<TimeOffRequestDto>(items, totalCount, page, pageSize);
    }

    public async Task<TimeOffRequestDto?> GetTimeOffRequestByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await _timeOffRepository.GetQueryable()
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (request == null) return null;

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        return MapToDto(request, user?.FullName);
    }

    public async Task<TimeOffRequestDto> CreateTimeOffRequestAsync(Guid userId, CreateTimeOffRequestDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) throw new InvalidOperationException("User not found");

        // Validate: One request per month
        var existingRequest = await _timeOffRepository.GetQueryable()
            .Where(r => r.UserId == userId && r.Date.Month == dto.Date.Month && r.Date.Year == dto.Date.Year && r.Status != RequestStatus.Rejected) 
            .AnyAsync(cancellationToken);

        if (existingRequest)
        {
            throw new InvalidOperationException("You can only submit one time off request per month.");
        }

        var request = new TimeOffRequest
        {
            UserId = userId,
            Date = dto.Date,
            StartTime = dto.StartTime,
            Reason = dto.Reason,
            Status = RequestStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            ManagerId = user.ManagerId
        };

        await _timeOffRepository.AddAsync(request, cancellationToken);
        await _timeOffRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(request, user.FullName);
    }

    public async Task<bool> SubmitTimeOffRequestAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await _timeOffRepository.GetByIdAsync(id, cancellationToken);
        if (request == null || request.Status != RequestStatus.Draft) return false;

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null) return false;

        if (user.Role == RoleType.Manager || user.Role == RoleType.HR || user.Role == RoleType.Admin)
        {
            request.Status = RequestStatus.PendingHR;
        }
        else
        {
            request.Status = RequestStatus.Submitted;
        }

        request.SubmittedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;

        _timeOffRepository.Update(request);
        await _timeOffRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteTimeOffRequestAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await _timeOffRepository.GetByIdAsync(id, cancellationToken);
        if (request == null || request.Status != RequestStatus.Draft) return false;

        _timeOffRepository.Delete(request);
        await _timeOffRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ApproveTimeOffRequestAsync(Guid id, Guid approverId, CancellationToken cancellationToken = default)
    {
        var request = await _timeOffRepository.GetByIdAsync(id, cancellationToken);
        if (request == null) return false;

        var approver = await _userRepository.GetByIdAsync(approverId, cancellationToken);
        if (approver == null) return false;

        bool isHROrAdmin = approver.Role == RoleType.HR || approver.Role == RoleType.Admin;
        bool isManager = approver.Role == RoleType.Manager;

        if (request.Status == RequestStatus.Submitted && (isManager || isHROrAdmin))
        {
            request.ManagerId = approverId;
            if (isHROrAdmin)
            {
                request.Status = RequestStatus.Approved;
                request.ApprovedByHRId = approverId;
                request.ApprovedAt = DateTime.UtcNow;
            }
            else
            {
                request.Status = RequestStatus.PendingHR;
            }
            
            request.UpdatedAt = DateTime.UtcNow;
            _timeOffRepository.Update(request);
            await _timeOffRepository.SaveChangesAsync(cancellationToken);

            await _approvalLogService.LogApprovalAsync(id, RequestType.TimeOff, approverId, true, "Approved", cancellationToken); // Using Overtime type or new type?
            // Need to check RequestType enum. 
            // If TimeOff is not in RequestType enum, I should add it. For now I might map it to Overtime or add it.
            // I'll assume I need to add it to Enum.
            
            return true;
        }

        if (request.Status == RequestStatus.PendingHR && isHROrAdmin)
        {
            request.Status = RequestStatus.Approved;
            request.ApprovedByHRId = approverId;
            request.ApprovedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;

            _timeOffRepository.Update(request);
            await _timeOffRepository.SaveChangesAsync(cancellationToken);
            
            await _approvalLogService.LogApprovalAsync(id, RequestType.TimeOff, approverId, true, "Final Approval", cancellationToken); 
            return true;
        }

        return false;
    }

    public async Task<bool> RejectTimeOffRequestAsync(Guid id, Guid approverId, string reason, CancellationToken cancellationToken = default)
    {
        var request = await _timeOffRepository.GetByIdAsync(id, cancellationToken);
        if (request == null) return false;

        var approver = await _userRepository.GetByIdAsync(approverId, cancellationToken);
        if (approver == null) return false;

        bool isHROrAdmin = approver.Role == RoleType.HR || approver.Role == RoleType.Admin;
        bool isManager = approver.Role == RoleType.Manager;

        bool canReject = false;
        if (request.Status == RequestStatus.Submitted && (isManager || isHROrAdmin)) canReject = true;
        if (request.Status == RequestStatus.PendingHR && isHROrAdmin) canReject = true;

        if (!canReject) return false;

        request.Status = RequestStatus.Rejected;
        request.RejectionReason = reason;
        request.UpdatedAt = DateTime.UtcNow;

        _timeOffRepository.Update(request);
        await _timeOffRepository.SaveChangesAsync(cancellationToken);

        await _approvalLogService.LogApprovalAsync(id, RequestType.TimeOff, approverId, false, reason, cancellationToken);

        return true;
    }

    public async Task<IEnumerable<TimeOffRequestDto>> GetPendingForManagerAsync(Guid managerId, CancellationToken cancellationToken = default)
    {
         var subordinates = await _userRepository.GetQueryable()
            .Where(u => u.ManagerId == managerId)
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync(cancellationToken);

        var subordinateIds = subordinates.Select(s => s.Id).ToList();
        var userNames = subordinates.ToDictionary(s => s.Id, s => s.FullName);

        var requests = await _timeOffRepository.GetQueryable()
            .Where(r => subordinateIds.Contains(r.UserId) && r.Status == RequestStatus.Submitted)
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync(cancellationToken);

        return requests.Select(r => MapToDto(r, userNames.TryGetValue(r.UserId, out var name) ? name : null));
    }

    public async Task<IEnumerable<TimeOffRequestDto>> GetPendingForHRAsync(CancellationToken cancellationToken = default)
    {
        var requests = await _timeOffRepository.GetQueryable()
            .Where(r => r.Status == RequestStatus.PendingHR || r.Status == RequestStatus.Submitted)
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync(cancellationToken);

        var userIds = requests.Select(r => r.UserId).Distinct().ToList();
        var users = await _userRepository.GetQueryable()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync(cancellationToken);
        var userNames = users.ToDictionary(u => u.Id, u => u.FullName);

        return requests.Select(r => MapToDto(r, userNames.TryGetValue(r.UserId, out var name) ? name : null));
    }

    private TimeOffRequestDto MapToDto(TimeOffRequest r, string? userName)
    {
        return new TimeOffRequestDto
        {
            Id = r.Id,
            UserId = r.UserId,
            UserName = userName,
            Date = r.Date,
            StartTime = r.StartTime,
            Reason = r.Reason,
            Status = r.Status,
            SubmittedAt = r.SubmittedAt,
            ApprovedAt = r.ApprovedAt,
            ApprovedByHRName = null, // Can fetch if needed
            RejectionReason = r.RejectionReason,
            CreatedAt = r.CreatedAt
        };
    }
}
