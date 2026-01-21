using HrSystem.Application.Common.Models;
using HrSystem.Application.DTOs.WorkFromHome;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using HrSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using HrSystem.Application.Services.ApprovalLogs;

namespace HrSystem.Application.Services.WorkFromHome;

public class WorkFromHomeService : IWorkFromHomeService
{
    private readonly IRepository<WorkFromHomeRequest> _wfhRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IApprovalLogService _approvalLogService;

    public WorkFromHomeService(
        IRepository<WorkFromHomeRequest> wfhRepository,
        IRepository<User> userRepository,
        IApprovalLogService approvalLogService)
    {
        _wfhRepository = wfhRepository;
        _userRepository = userRepository;
        _approvalLogService = approvalLogService;
    }

    public async Task<WorkFromHomeRequestDto> CreateWorkFromHomeRequestAsync(
        Guid userId,
        CreateWorkFromHomeRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new Exception("User not found");

        var request = new WorkFromHomeRequest
        {
            UserId = userId,
            FromDate = dto.FromDate.Date, // Ensure date only
            ToDate = dto.ToDate.Date, // Ensure date only
            Reason = dto.Reason,
            Status = RequestStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            ManagerId = user.ManagerId
        };

        // Calculate total days
        request.TotalDays = (request.ToDate - request.FromDate).Days + 1;

        await _wfhRepository.AddAsync(request, cancellationToken);
        await _wfhRepository.SaveChangesAsync(cancellationToken);

        return MapToDto(request, user.FullName);
    }

    public async Task<bool> SubmitWorkFromHomeRequestAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await _wfhRepository.GetByIdAsync(id, cancellationToken);
        
        if (request == null || request.Status != RequestStatus.Draft)
            return false;

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null) return false;

        // If user is a Manager/HR/Admin, skip manager approval
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

        _wfhRepository.Update(request);
        await _wfhRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ApproveWorkFromHomeRequestAsync(Guid id, Guid approverId, CancellationToken cancellationToken = default)
    {
        var request = await _wfhRepository.GetByIdAsync(id, cancellationToken);
        if (request == null) return false;

        var approver = await _userRepository.GetByIdAsync(approverId, cancellationToken);
        if (approver == null) return false;

        bool isHROrAdmin = approver.Role == RoleType.HR || approver.Role == RoleType.Admin;
        bool isManager = approver.Role == RoleType.Manager;

        // Manager approval (from Submitted status)
        if (request.Status == RequestStatus.Submitted && (isManager || isHROrAdmin))
        {
            request.ManagerId = approverId;
            
            if (isHROrAdmin)
            {
                // HR can do both approvals at once
                request.Status = RequestStatus.Approved;
                request.ApprovedByHRId = approverId;
                request.ApprovedAt = DateTime.UtcNow;
            }
            else
            {
                // Manager approves, now needs HR
                request.Status = RequestStatus.PendingHR;
            }
            
            request.UpdatedAt = DateTime.UtcNow;
            _wfhRepository.Update(request);
            await _wfhRepository.SaveChangesAsync(cancellationToken);

            await _approvalLogService.LogApprovalAsync(
                id, RequestType.WorkFromHome, approverId, true, "Approved by Manager/HR", cancellationToken);

            return true;
        }

        // HR final approval (from PendingHR status)
        if (request.Status == RequestStatus.PendingHR && isHROrAdmin)
        {
            request.Status = RequestStatus.Approved;
            request.ApprovedByHRId = approverId;
            request.ApprovedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;

            _wfhRepository.Update(request);
            await _wfhRepository.SaveChangesAsync(cancellationToken);
            
            await _approvalLogService.LogApprovalAsync(
                id, RequestType.WorkFromHome, approverId, true, "Final Approval by HR", cancellationToken);

            return true;
        }

        return false;
    }

    public async Task<bool> RejectWorkFromHomeRequestAsync(Guid id, Guid approverId, string reason, CancellationToken cancellationToken = default)
    {
        var request = await _wfhRepository.GetByIdAsync(id, cancellationToken);
        if (request == null) return false;

        var approver = await _userRepository.GetByIdAsync(approverId, cancellationToken);
        if (approver == null) return false;

        bool isHROrAdmin = approver.Role == RoleType.HR || approver.Role == RoleType.Admin;
        bool isManager = approver.Role == RoleType.Manager;

        // Check if approver can reject this request
        bool canReject = false;
        if (request.Status == RequestStatus.Submitted && (isManager || isHROrAdmin))
            canReject = true;
        if (request.Status == RequestStatus.PendingHR && isHROrAdmin)
            canReject = true;

        if (!canReject)
            return false;

        request.Status = RequestStatus.Rejected;
        request.RejectionReason = reason;
        request.UpdatedAt = DateTime.UtcNow;

        _wfhRepository.Update(request);
        await _wfhRepository.SaveChangesAsync(cancellationToken);

        await _approvalLogService.LogApprovalAsync(
            id, RequestType.WorkFromHome, approverId, false, reason, cancellationToken);

        return true;
    }

    public async Task<bool> DeleteWorkFromHomeRequestAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await _wfhRepository.GetByIdAsync(id, cancellationToken);
        if (request == null || request.Status != RequestStatus.Draft)
            return false;

        _wfhRepository.Delete(request);
        await _wfhRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<PaginatedResult<WorkFromHomeRequestDto>> GetWorkFromHomeRequestsAsync(
        int page,
        int pageSize,
        RequestStatus? status,
        Guid? userId = null,
        IEnumerable<Guid>? userIds = null,
        CancellationToken cancellationToken = default)
    {
        var query = _wfhRepository.GetQueryable();
        
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

        // Get user names for all requests
        var userIdsReq = requests.Select(r => r.UserId).Distinct().ToList();
        var users = await _userRepository.GetQueryable()
            .Where(u => userIdsReq.Contains(u.Id))
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync(cancellationToken);
        var userNames = users.ToDictionary(u => u.Id, u => u.FullName);

        var items = requests.Select(r => 
        {
            var dto = MapToDto(r);
            dto.UserName = userNames.TryGetValue(r.UserId, out var name) ? name : null;
            return dto;
        }).ToList();

        return new PaginatedResult<WorkFromHomeRequestDto>(items, totalCount, page, pageSize);
    }

    public async Task<WorkFromHomeRequestDto?> GetWorkFromHomeRequestByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var request = await _wfhRepository.GetByIdAsync(id, cancellationToken);
        if (request == null) return null;

        var dto = MapToDto(request);
        
        // Get user name
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        dto.UserName = user?.FullName;

        return dto;
    }

    public async Task<IEnumerable<WorkFromHomeRequestDto>> GetPendingForManagerAsync(Guid managerId, CancellationToken cancellationToken = default)
    {
        // Get all users under this manager
        var subordinates = await _userRepository.GetQueryable()
            .Where(u => u.ManagerId == managerId)
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync(cancellationToken);

        var subordinateIds = subordinates.Select(s => s.Id).ToList();
        var userNames = subordinates.ToDictionary(s => s.Id, s => s.FullName);

        var requests = await _wfhRepository.GetQueryable()
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

    public async Task<IEnumerable<WorkFromHomeRequestDto>> GetPendingForHRAsync(CancellationToken cancellationToken = default)
    {
        var requests = await _wfhRepository.GetQueryable()
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

    private WorkFromHomeRequestDto MapToDto(WorkFromHomeRequest r, string? userName = null)
    {
        return new WorkFromHomeRequestDto
        {
            Id = r.Id,
            UserId = r.UserId,
            UserName = userName,
            FromDate = r.FromDate,
            ToDate = r.ToDate,
            TotalDays = r.TotalDays,
            Reason = r.Reason,
            Status = r.Status,
            ManagerId = r.ManagerId,
            ApprovedByHRId = r.ApprovedByHRId,
            SubmittedAt = r.SubmittedAt,
            ApprovedAt = r.ApprovedAt,
            RejectionReason = r.RejectionReason,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        };
    }
}
