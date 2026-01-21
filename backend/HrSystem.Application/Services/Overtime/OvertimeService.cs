using HrSystem.Application.Common.Interfaces;
using HrSystem.Application.Common.Models;
using HrSystem.Application.DTOs.Overtime;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using HrSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using HrSystem.Application.Services.ApprovalLogs;

namespace HrSystem.Application.Services.Overtime;

public class OvertimeService : IOvertimeService
{
    private readonly IRepository<OvertimeRequest> _overtimeRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IApprovalLogService _approvalLogService;

    public OvertimeService(
        IRepository<OvertimeRequest> overtimeRepository,
        IRepository<User> userRepository,
        IApprovalLogService approvalLogService)
    {
        _overtimeRepository = overtimeRepository;
        _userRepository = userRepository;
        _approvalLogService = approvalLogService;
    }

    public async Task<PaginatedResult<OvertimeRequestDto>> GetOvertimeRequestsAsync(
        int page,
        int pageSize,
        RequestStatus? status,
        string? search,
        Guid? userId = null,
        IEnumerable<Guid>? userIds = null,
        CancellationToken cancellationToken = default)
    {
        var query = _overtimeRepository.GetQueryable();

        if (userId.HasValue)
        {
            query = query.Where(o => o.UserId == userId.Value);
        }

        if (userIds != null && userIds.Any())
        {
            query = query.Where(o => userIds.Contains(o.UserId));
        }

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(o => o.Reason.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var requests = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Get user names for all requests
        var userIdsReq = requests.Select(r => r.UserId).Distinct().ToList();
        var users = await _userRepository.GetQueryable()
            .Where(u => userIdsReq.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync(cancellationToken);
        var userNames = users.ToDictionary(u => u.Id, u => u.FullName);

        var items = requests.Select(o => new OvertimeRequestDto
        {
            Id = o.Id,
            UserId = o.UserId,
            UserName = userNames.TryGetValue(o.UserId, out var name) ? name : null,
            StartDateTime = o.StartDateTime,
            EndDateTime = o.EndDateTime,
            HoursWorked = o.HoursWorked,
            Reason = o.Reason,
            Status = o.Status,
            ManagerId = o.ManagerId,
            ApprovedByHRId = o.ApprovedByHRId,
            SubmittedAt = o.SubmittedAt,
            ApprovedAt = o.ApprovedAt,
            RejectionReason = o.RejectionReason,
            IsOverridden = o.IsOverridden,
            CreatedAt = o.CreatedAt,
            UpdatedAt = o.UpdatedAt
        }).ToList();

        return new PaginatedResult<OvertimeRequestDto>(items, totalCount, page, pageSize);
    }

    public async Task<OvertimeRequestDto?> GetOvertimeRequestByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var overtime = await _overtimeRepository.GetQueryable()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (overtime == null)
            return null;

        var dto = MapToDto(overtime);
        
        // Get user name
        var user = await _userRepository.GetByIdAsync(overtime.UserId, cancellationToken);
        dto.UserName = user?.FullName;

        return dto;
    }

    /// <summary>
    /// Create overtime request with workflow logic
    /// </summary>
    public async Task<OvertimeRequestDto> CreateOvertimeRequestAsync(
        Guid userId,
        CreateOvertimeRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException("User not found");

        var overtime = new OvertimeRequest
        {
            UserId = userId,
            StartDateTime = dto.StartDateTime,
            EndDateTime = dto.EndDateTime,
            HoursWorked = dto.HoursWorked,
            Reason = dto.Reason,
            Status = RequestStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            ManagerId = user.ManagerId
        };

        await _overtimeRepository.AddAsync(overtime, cancellationToken);
        await _overtimeRepository.SaveChangesAsync(cancellationToken);

        var result = MapToDto(overtime);
        result.UserName = user.FullName;
        return result;
    }

    /// <summary>
    /// Submit a draft request - workflow depends on user role
    /// </summary>
    public async Task<bool> SubmitOvertimeRequestAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var overtime = await _overtimeRepository.GetByIdAsync(id, cancellationToken);

        if (overtime == null || overtime.Status != RequestStatus.Draft)
            return false;

        var user = await _userRepository.GetByIdAsync(overtime.UserId, cancellationToken);
        if (user == null) return false;

        // If user is Manager/HR/Admin, skip manager approval
        if (user.Role == RoleType.Manager || user.Role == RoleType.HR || user.Role == RoleType.Admin)
        {
            overtime.Status = RequestStatus.PendingHR;
        }
        else
        {
            overtime.Status = RequestStatus.Submitted;
        }

        overtime.SubmittedAt = DateTime.UtcNow;
        overtime.UpdatedAt = DateTime.UtcNow;

        _overtimeRepository.Update(overtime);
        await _overtimeRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteOvertimeRequestAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var overtime = await _overtimeRepository.GetByIdAsync(id, cancellationToken);

        if (overtime == null || overtime.Status != RequestStatus.Draft)
            return false;

        _overtimeRepository.Delete(overtime);
        await _overtimeRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// Approve overtime request with workflow:
    /// - Manager approves: Submitted -> PendingHR
    /// - HR approves: PendingHR -> Approved (or can approve from Submitted directly)
    /// </summary>
    public async Task<bool> ApproveOvertimeRequestAsync(Guid id, Guid approverId, CancellationToken cancellationToken = default)
    {
        var overtime = await _overtimeRepository.GetByIdAsync(id, cancellationToken);
        if (overtime == null) return false;

        var approver = await _userRepository.GetByIdAsync(approverId, cancellationToken);
        if (approver == null) return false;

        bool isHROrAdmin = approver.Role == RoleType.HR || approver.Role == RoleType.Admin;
        bool isManager = approver.Role == RoleType.Manager;

        // Manager approval (from Submitted status)
        if (overtime.Status == RequestStatus.Submitted && (isManager || isHROrAdmin))
        {
            overtime.ManagerId = approverId;

            if (isHROrAdmin)
            {
                // HR can do both approvals at once
                overtime.Status = RequestStatus.Approved;
                overtime.ApprovedByHRId = approverId;
                overtime.ApprovedAt = DateTime.UtcNow;
            }
            else
            {
                // Manager approves, now needs HR
                overtime.Status = RequestStatus.PendingHR;
            }

            overtime.UpdatedAt = DateTime.UtcNow;
            _overtimeRepository.Update(overtime);
            await _overtimeRepository.SaveChangesAsync(cancellationToken);

            await _approvalLogService.LogApprovalAsync(
                id, RequestType.Overtime, approverId, true, "Approved by Manager/HR", cancellationToken);

            return true;
        }

        // HR final approval (from PendingHR status)
        if (overtime.Status == RequestStatus.PendingHR && isHROrAdmin)
        {
            overtime.Status = RequestStatus.Approved;
            overtime.ApprovedByHRId = approverId;
            overtime.ApprovedAt = DateTime.UtcNow;
            overtime.UpdatedAt = DateTime.UtcNow;

            _overtimeRepository.Update(overtime);
            await _overtimeRepository.SaveChangesAsync(cancellationToken);

            await _approvalLogService.LogApprovalAsync(
                id, RequestType.Overtime, approverId, true, "Final Approval by HR", cancellationToken);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Reject overtime request - can be done by Manager (from Submitted) or HR (from any pending status)
    /// </summary>
    public async Task<bool> RejectOvertimeRequestAsync(Guid id, Guid approverId, string reason, CancellationToken cancellationToken = default)
    {
        var overtime = await _overtimeRepository.GetByIdAsync(id, cancellationToken);
        if (overtime == null) return false;

        var approver = await _userRepository.GetByIdAsync(approverId, cancellationToken);
        if (approver == null) return false;

        bool isHROrAdmin = approver.Role == RoleType.HR || approver.Role == RoleType.Admin;
        bool isManager = approver.Role == RoleType.Manager;

        // Check if approver can reject this request
        bool canReject = false;
        if (overtime.Status == RequestStatus.Submitted && (isManager || isHROrAdmin))
            canReject = true;
        if (overtime.Status == RequestStatus.PendingHR && isHROrAdmin)
            canReject = true;

        if (!canReject)
            return false;

        overtime.Status = RequestStatus.Rejected;
        overtime.RejectionReason = reason;
        overtime.UpdatedAt = DateTime.UtcNow;

        _overtimeRepository.Update(overtime);
        await _overtimeRepository.SaveChangesAsync(cancellationToken);

        await _approvalLogService.LogApprovalAsync(
            id, RequestType.Overtime, approverId, false, reason, cancellationToken);

        return true;
    }

    public async Task<OvertimeRequestDto?> UpdateOvertimeRequestAsync(Guid id, UpdateOvertimeRequestDto dto, CancellationToken cancellationToken = default)
    {
        var overtime = await _overtimeRepository.GetByIdAsync(id, cancellationToken);
        if (overtime == null) return null;

        if (overtime.Status != RequestStatus.Draft)
            throw new InvalidOperationException("Only draft requests can be updated.");

        overtime.StartDateTime = dto.StartDateTime;
        overtime.EndDateTime = dto.EndDateTime;
        overtime.HoursWorked = dto.HoursWorked;
        overtime.Reason = dto.Reason;
        overtime.UpdatedAt = DateTime.UtcNow;

        _overtimeRepository.Update(overtime);
        await _overtimeRepository.SaveChangesAsync(cancellationToken);

        var user = await _userRepository.GetByIdAsync(overtime.UserId, cancellationToken);
        var result = MapToDto(overtime);
        result.UserName = user?.FullName;
        return result;
    }

    /// <summary>
    /// Get pending requests for a manager to approve
    /// </summary>
    public async Task<IEnumerable<OvertimeRequestDto>> GetPendingForManagerAsync(Guid managerId, CancellationToken cancellationToken = default)
    {
        // Get all users under this manager with their names
        var subordinates = await _userRepository.GetQueryable()
            .Where(u => u.ManagerId == managerId)
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync(cancellationToken);

        var subordinateIds = subordinates.Select(s => s.Id).ToList();
        var userNames = subordinates.ToDictionary(s => s.Id, s => s.FullName);

        var requests = await _overtimeRepository.GetQueryable()
            .Where(r => subordinateIds.Contains(r.UserId) && r.Status == RequestStatus.Submitted)
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync(cancellationToken);

        return requests.Select(r =>
        {
            var dto = MapToDto(r);
            dto.UserName = userNames.TryGetValue(r.UserId, out var name) ? name : null;
            return dto;
        });
    }

    /// <summary>
    /// Get pending requests for HR to approve (includes both PendingHR and Submitted - HR can approve both)
    /// </summary>
    public async Task<IEnumerable<OvertimeRequestDto>> GetPendingForHRAsync(CancellationToken cancellationToken = default)
    {
        var requests = await _overtimeRepository.GetQueryable()
            .Where(r => r.Status == RequestStatus.PendingHR || r.Status == RequestStatus.Submitted)
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync(cancellationToken);

        // Get user names for all requests
        var userIds = requests.Select(r => r.UserId).Distinct().ToList();
        var users = await _userRepository.GetQueryable()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync(cancellationToken);
        var userNames = users.ToDictionary(u => u.Id, u => u.FullName);

        return requests.Select(r =>
        {
            var dto = MapToDto(r);
            dto.UserName = userNames.TryGetValue(r.UserId, out var name) ? name : null;
            return dto;
        });
    }

    private OvertimeRequestDto MapToDto(OvertimeRequest o)
    {
        return new OvertimeRequestDto
        {
            Id = o.Id,
            UserId = o.UserId,
            StartDateTime = o.StartDateTime,
            EndDateTime = o.EndDateTime,
            HoursWorked = o.HoursWorked,
            Reason = o.Reason,
            Status = o.Status,
            ManagerId = o.ManagerId,
            ApprovedByHRId = o.ApprovedByHRId,
            SubmittedAt = o.SubmittedAt,
            ApprovedAt = o.ApprovedAt,
            RejectionReason = o.RejectionReason,
            IsOverridden = o.IsOverridden,
            CreatedAt = o.CreatedAt,
            UpdatedAt = o.UpdatedAt
        };
    }
}
