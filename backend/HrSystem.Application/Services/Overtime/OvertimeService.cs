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
            //.Include(o => o.User);

        // Apply user filter
        if (userId.HasValue)
        {
            query = query.Where(o => o.UserId == userId.Value);
        }

        // Apply status filter
        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(o => //o.User.FullName.Contains(search) || 
                                    o.Reason.Contains(search));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OvertimeRequestDto
            {
                Id = o.Id,
                UserId = o.UserId,
                //UserName = o.User.FullName,
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
            //.Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (overtime == null)
            return null;

        return new OvertimeRequestDto
        {
            Id = overtime.Id,
            UserId = overtime.UserId,
            //UserName = overtime.User?.FullName,
            StartDateTime = overtime.StartDateTime,
            EndDateTime = overtime.EndDateTime,
            HoursWorked = overtime.HoursWorked,
            Reason = overtime.Reason,
            Status = overtime.Status,
            ManagerId = overtime.ManagerId,
            ApprovedByHRId = overtime.ApprovedByHRId,
            SubmittedAt = overtime.SubmittedAt,
            ApprovedAt = overtime.ApprovedAt,
            RejectionReason = overtime.RejectionReason,
            IsOverridden = overtime.IsOverridden,
            CreatedAt = overtime.CreatedAt,
            UpdatedAt = overtime.UpdatedAt
        };
    }

    public async Task<OvertimeRequestDto> CreateOvertimeRequestAsync(
        Guid userId,
        CreateOvertimeRequestDto dto,
        CancellationToken cancellationToken = default)
    {
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

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        return new OvertimeRequestDto
        {
            Id = overtime.Id,
            UserId = overtime.UserId,
            UserName = user?.FullName,
            StartDateTime = overtime.StartDateTime,
            EndDateTime = overtime.EndDateTime,
            HoursWorked = overtime.HoursWorked,
            Reason = overtime.Reason,
            Status = overtime.Status,
            CreatedAt = overtime.CreatedAt
        };
    }

    public async Task<bool> SubmitOvertimeRequestAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var overtime = await _overtimeRepository.GetByIdAsync(id, cancellationToken);

        if (overtime == null || overtime.Status != RequestStatus.Draft)
            return false;

        overtime.Status = RequestStatus.Submitted;
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
    public async Task<bool> ApproveOvertimeRequestAsync(Guid id, Guid approverId, CancellationToken cancellationToken = default)
    {
        var overtime = await _overtimeRepository.GetByIdAsync(id, cancellationToken);

        if (overtime == null || overtime.Status != RequestStatus.Submitted)
            return false;

        overtime.Status = RequestStatus.Approved;
        overtime.ApprovedByHRId = approverId; // Assuming HR is the approver for Overtime
        overtime.ApprovedAt = DateTime.UtcNow;
        overtime.UpdatedAt = DateTime.UtcNow;

        _overtimeRepository.Update(overtime);
        await _overtimeRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> RejectOvertimeRequestAsync(Guid id, Guid approverId, string reason, CancellationToken cancellationToken = default)
    {
        var overtime = await _overtimeRepository.GetByIdAsync(id, cancellationToken);

        if (overtime == null || overtime.Status != RequestStatus.Submitted)
            return false;

        overtime.Status = RequestStatus.Rejected;
        overtime.ApprovedByHRId = approverId; // We record who rejected it
        overtime.RejectionReason = reason;
        overtime.ApprovedAt = DateTime.UtcNow; // Or maybe RejectedAt if exists, but reusing ApprovedAt as 'DecisionAt' is common
        overtime.UpdatedAt = DateTime.UtcNow;

        _overtimeRepository.Update(overtime);
        await _overtimeRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
