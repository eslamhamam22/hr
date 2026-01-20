using HrSystem.Application.Common.Interfaces;
using HrSystem.Application.Common.Models;
using HrSystem.Application.DTOs.Overtime;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using HrSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Services.Overtime;

public class OvertimeService : IOvertimeService
{
    private readonly IRepository<OvertimeRequest> _overtimeRepository;
    private readonly IRepository<User> _userRepository;

    public OvertimeService(
        IRepository<OvertimeRequest> overtimeRepository,
        IRepository<User> userRepository)
    {
        _overtimeRepository = overtimeRepository;
        _userRepository = userRepository;
    }

    public async Task<PaginatedResult<OvertimeRequestDto>> GetOvertimeRequestsAsync(
        int page,
        int pageSize,
        RequestStatus? status,
        string? search,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _overtimeRepository.GetQueryable();

        if (userId.HasValue)
        {
            query = query.Where(o => o.UserId == userId.Value);
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

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OvertimeRequestDto
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
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResult<OvertimeRequestDto>(items, totalCount, page, pageSize);
    }

    public async Task<OvertimeRequestDto?> GetOvertimeRequestByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var overtime = await _overtimeRepository.GetQueryable()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (overtime == null)
            return null;

        return MapToDto(overtime);
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
            CreatedAt = DateTime.UtcNow
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

        return true;
    }

    /// <summary>
    /// Get pending requests for a manager to approve
    /// </summary>
    public async Task<IEnumerable<OvertimeRequestDto>> GetPendingForManagerAsync(Guid managerId, CancellationToken cancellationToken = default)
    {
        var subordinateIds = await _userRepository.GetQueryable()
            .Where(u => u.ManagerId == managerId)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        var requests = await _overtimeRepository.GetQueryable()
            .Where(r => subordinateIds.Contains(r.UserId) && r.Status == RequestStatus.Submitted)
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync(cancellationToken);

        return requests.Select(MapToDto);
    }

    /// <summary>
    /// Get pending requests for HR to approve
    /// </summary>
    public async Task<IEnumerable<OvertimeRequestDto>> GetPendingForHRAsync(CancellationToken cancellationToken = default)
    {
        var requests = await _overtimeRepository.GetQueryable()
            .Where(r => r.Status == RequestStatus.PendingHR)
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync(cancellationToken);

        return requests.Select(MapToDto);
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
