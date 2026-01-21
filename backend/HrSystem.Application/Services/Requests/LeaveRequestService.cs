using HrSystem.Application.Common.Interfaces;
using HrSystem.Application.Common.Models;
using HrSystem.Application.DTOs.Requests;
using HrSystem.Application.Services.Requests;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using HrSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using HrSystem.Application.Services.ApprovalLogs;

namespace HrSystem.Application.Services.Requests;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly IRepository<LeaveRequest> _leaveRequestRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IApprovalLogService _approvalLogService;

    public LeaveRequestService(
        IRepository<LeaveRequest> leaveRequestRepository,
        IRepository<User> userRepository,
        IApprovalLogService approvalLogService)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _userRepository = userRepository;
        _approvalLogService = approvalLogService;
    }

    /// <summary>
    /// Create a leave request. Status depends on who creates it:
    /// - Employee creates for self: Draft (needs to submit) or Submitted (goes to manager)
    /// - Manager/HR creates for employee: Goes directly to PendingHR (skips manager approval)
    /// </summary>
    public async Task<LeaveRequestDto?> CreateLeaveRequestAsync(
        Guid requestorUserId,
        Guid targetUserId,
        int leaveTypeId, 
        DateTime startDate, 
        DateTime endDate, 
        string reason,
        bool submitImmediately = false,
        CancellationToken cancellationToken = default)
    {
        // ... (existing implementation) ...
        var requestor = await _userRepository.GetByIdAsync(requestorUserId, cancellationToken);
        var targetUser = await _userRepository.GetByIdAsync(targetUserId, cancellationToken);
        
        if (requestor == null || targetUser == null)
            return null;

        var request = new LeaveRequest
        {
            UserId = targetUserId,
            LeaveType = (LeaveType)leaveTypeId,
            StartDate = startDate,
            EndDate = endDate,
            Reason = reason,
            CreatedAt = DateTime.UtcNow,
            ManagerId = requestor.ManagerId
        };

        // Calculate total days
        request.TotalDays = (endDate - startDate).Days + 1;

        // Determine initial status based on who creates the request
        bool isCreatorManager = requestor.Role == RoleType.Manager || requestor.Role == RoleType.HR || requestor.Role == RoleType.Admin;
        bool isCreatingForSelf = requestorUserId == targetUserId;

        if (isCreatingForSelf)
        {
            // Employee creating for themselves
            if (isCreatorManager)
            {
                // Manager/HR creating for themselves - goes directly to HR for final approval
                request.Status = submitImmediately ? RequestStatus.PendingHR : RequestStatus.Draft;
                request.SubmittedAt = submitImmediately ? DateTime.UtcNow : null;
            }
            else
            {
                // Regular employee - needs manager approval first
                request.Status = submitImmediately ? RequestStatus.Submitted : RequestStatus.Draft;
                request.SubmittedAt = submitImmediately ? DateTime.UtcNow : null;
            }
        }
        else
        {
            // Creating for someone else (Manager/HR creating for employee)
            if (requestor.Role == RoleType.HR || requestor.Role == RoleType.Admin)
            {
                // HR creates for employee - auto-approved
                request.Status = RequestStatus.Approved;
                request.ApprovedByHRId = requestorUserId;
                request.ApprovedAt = DateTime.UtcNow;
                request.SubmittedAt = DateTime.UtcNow;
            }
            else if (requestor.Role == RoleType.Manager)
            {
                // Manager creates for their employee - goes to HR for final approval
                request.Status = submitImmediately ? RequestStatus.PendingHR : RequestStatus.Draft;
                request.ManagerId = requestorUserId;
                request.SubmittedAt = submitImmediately ? DateTime.UtcNow : null;
            }
            else
            {
                // Regular employee cannot create for others
                return null;
            }
        }

        await _leaveRequestRepository.AddAsync(request, cancellationToken);
        await _leaveRequestRepository.SaveChangesAsync(cancellationToken);

        if (request.Status == RequestStatus.Approved && request.ApprovedByHRId.HasValue)
        {
             await _approvalLogService.LogApprovalAsync(
                request.Id, RequestType.Leave, request.ApprovedByHRId.Value, true, "Auto-approved by Creator", cancellationToken);
        }

        return MapToDto(request);
    }

    /// <summary>
    /// Legacy method for backward compatibility
    /// </summary>
    public async Task<Guid?> CreateLeaveRequestAsync(
        Guid userId, 
        int leaveTypeId, 
        DateTime startDate, 
        DateTime endDate, 
        string reason, 
        CancellationToken cancellationToken = default)
    {
        var result = await CreateLeaveRequestAsync(userId, userId, leaveTypeId, startDate, endDate, reason, false, cancellationToken);
        return result?.Id;
    }

    /// <summary>
    /// Submit a draft request - moves to Submitted (waiting for manager) or PendingHR (if manager)
    /// </summary>
    public async Task<bool> SubmitLeaveRequestAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        var request = await _leaveRequestRepository.GetByIdAsync(requestId, cancellationToken);
        
        if (request == null || request.Status != RequestStatus.Draft)
            return false;

        // Get the user to determine workflow
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

        _leaveRequestRepository.Update(request);
        await _leaveRequestRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// Manager approves - moves to PendingHR
    /// HR approves - moves to Approved (final)
    /// </summary>
    public async Task<bool> ApproveLeaveRequestAsync(Guid requestId, Guid approverUserId, CancellationToken cancellationToken = default)
    {
        var request = await _leaveRequestRepository.GetByIdAsync(requestId, cancellationToken);
        if (request == null) return false;

        var approver = await _userRepository.GetByIdAsync(approverUserId, cancellationToken);
        if (approver == null) return false;

        bool isHROrAdmin = approver.Role == RoleType.HR || approver.Role == RoleType.Admin;
        bool isManager = approver.Role == RoleType.Manager;

        // Manager approval (from Submitted status)
        if (request.Status == RequestStatus.Submitted && (isManager || isHROrAdmin))
        {
            request.ManagerId = approverUserId;
            
            if (isHROrAdmin)
            {
                // HR can do both approvals at once
                request.Status = RequestStatus.Approved;
                request.ApprovedByHRId = approverUserId;
                request.ApprovedAt = DateTime.UtcNow;
            }
            else
            {
                // Manager approves, now needs HR
                request.Status = RequestStatus.PendingHR;
            }
            
            request.UpdatedAt = DateTime.UtcNow;
            _leaveRequestRepository.Update(request);
            await _leaveRequestRepository.SaveChangesAsync(cancellationToken);

            await _approvalLogService.LogApprovalAsync(
                requestId, RequestType.Leave, approverUserId, true, "Approved by Manager/HR", cancellationToken);

            return true;
        }

        // HR final approval (from PendingHR status)
        if (request.Status == RequestStatus.PendingHR && isHROrAdmin)
        {
            request.Status = RequestStatus.Approved;
            request.ApprovedByHRId = approverUserId;
            request.ApprovedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;

            _leaveRequestRepository.Update(request);
            await _leaveRequestRepository.SaveChangesAsync(cancellationToken);
            
            await _approvalLogService.LogApprovalAsync(
                requestId, RequestType.Leave, approverUserId, true, "Final Approval by HR", cancellationToken);

            return true;
        }

        return false;
    }

    /// <summary>
    /// Reject request - can be done by Manager (from Submitted) or HR (from any pending status)
    /// </summary>
    public async Task<bool> RejectLeaveRequestAsync(Guid requestId, Guid approverUserId, string rejectionReason, CancellationToken cancellationToken = default)
    {
        var request = await _leaveRequestRepository.GetByIdAsync(requestId, cancellationToken);
        if (request == null) return false;

        var approver = await _userRepository.GetByIdAsync(approverUserId, cancellationToken);
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
        request.RejectionReason = rejectionReason;
        request.UpdatedAt = DateTime.UtcNow;

        _leaveRequestRepository.Update(request);
        await _leaveRequestRepository.SaveChangesAsync(cancellationToken);

        await _approvalLogService.LogApprovalAsync(
            requestId, RequestType.Leave, approverUserId, false, rejectionReason, cancellationToken);

        return true;
    }

    public async Task<PaginatedResult<LeaveRequestDto>> GetLeaveRequestsAsync(
        int page, 
        int pageSize, 
        RequestStatus? status, 
        Guid? userId = null,
        IEnumerable<Guid>? userIds = null,
        CancellationToken cancellationToken = default)
    {
         var query = _leaveRequestRepository.GetQueryable();
         
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

         var items = requests.Select(r => new LeaveRequestDto
         {
             Id = r.Id,
             UserId = r.UserId,
             UserName = userNames.TryGetValue(r.UserId, out var name) ? name : null,
             LeaveTypeId = (int)r.LeaveType,
             LeaveTypeName = r.LeaveType.ToString(),
             StartDate = r.StartDate,
             EndDate = r.EndDate,
             Reason = r.Reason,
             Status = r.Status,
             ManagerId = r.ManagerId,
             ApprovedByHRId = r.ApprovedByHRId,
             SubmittedAt = r.SubmittedAt,
             ApprovedAt = r.ApprovedAt,
             RejectionReason = r.RejectionReason,
             CreatedAt = r.CreatedAt,
             UpdatedAt = r.UpdatedAt
         }).ToList();

         return new PaginatedResult<LeaveRequestDto>(items, totalCount, page, pageSize);
    }

    /// <summary>
    /// Get pending requests for a manager to approve
    /// </summary>
    public async Task<IEnumerable<LeaveRequestDto>> GetPendingForManagerAsync(Guid managerId, CancellationToken cancellationToken = default)
    {
        // Get all users under this manager with their names
        var subordinates = await _userRepository.GetQueryable()
            .Where(u => u.ManagerId == managerId)
            .Select(u => new { u.Id, u.FullName })
            .ToListAsync(cancellationToken);

        var subordinateIds = subordinates.Select(s => s.Id).ToList();
        var userNames = subordinates.ToDictionary(s => s.Id, s => s.FullName);

        var requests = await _leaveRequestRepository.GetQueryable()
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
    public async Task<IEnumerable<LeaveRequestDto>> GetPendingForHRAsync(CancellationToken cancellationToken = default)
    {
        var requests = await _leaveRequestRepository.GetQueryable()
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

    public async Task<LeaveRequestDto?> GetLeaveRequestByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var r = await _leaveRequestRepository.GetByIdAsync(id, cancellationToken);
        if (r == null) return null;

        var dto = MapToDto(r);
        
        // Get user name
        var user = await _userRepository.GetByIdAsync(r.UserId, cancellationToken);
        dto.UserName = user?.FullName;

        return dto;
    }

    private LeaveRequestDto MapToDto(LeaveRequest r)
    {
        return new LeaveRequestDto
        {
            Id = r.Id,
            UserId = r.UserId,
            LeaveTypeId = (int)r.LeaveType,
            LeaveTypeName = r.LeaveType.ToString(),
            StartDate = r.StartDate,
            EndDate = r.EndDate,
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
