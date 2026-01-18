using HrSystem.Application.Common.Interfaces;
using HrSystem.Application.Common.Models;
using HrSystem.Application.DTOs.Requests;
using HrSystem.Application.Services.Requests;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using HrSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Services.Requests;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly IRepository<LeaveRequest> _leaveRequestRepository;
    private readonly IRepository<User> _userRepository;

    public LeaveRequestService(
        IRepository<LeaveRequest> leaveRequestRepository,
        IRepository<User> userRepository)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _userRepository = userRepository;
    }

    public async Task<bool> CreateLeaveRequestAsync(
        Guid userId, 
        int leaveTypeId, 
        DateTime startDate, 
        DateTime endDate, 
        string reason, 
        CancellationToken cancellationToken = default)
    {
        // Simple logic for now, ideally validate balance etc but skipping for brevity as per "completing APIs"
        var request = new LeaveRequest
        {
            UserId = userId,
            LeaveType = (LeaveType)leaveTypeId,
            StartDate = startDate,
            EndDate = endDate,
            Reason = reason,
            Status = RequestStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        // Calculate total days (simple)
        request.TotalDays = (endDate - startDate).Days + 1;

        await _leaveRequestRepository.AddAsync(request, cancellationToken);
        await _leaveRequestRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> SubmitLeaveRequestAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        var request = await _leaveRequestRepository.GetByIdAsync(requestId, cancellationToken);
        
        if (request == null || request.Status != RequestStatus.Draft)
            return false;

        request.Status = RequestStatus.Submitted;
        request.SubmittedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;

        _leaveRequestRepository.Update(request);
        await _leaveRequestRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ApproveLeaveRequestAsync(Guid requestId, Guid approverUserId, CancellationToken cancellationToken = default)
    {
        var request = await _leaveRequestRepository.GetByIdAsync(requestId, cancellationToken);

        if (request == null || request.Status != RequestStatus.Submitted)
            return false;

        request.Status = RequestStatus.Approved;
        request.ApprovedByHRId = approverUserId;
        request.ApprovedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;

        _leaveRequestRepository.Update(request);
        await _leaveRequestRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> RejectLeaveRequestAsync(Guid requestId, Guid approverUserId, string rejectionReason, CancellationToken cancellationToken = default)
    {
        var request = await _leaveRequestRepository.GetByIdAsync(requestId, cancellationToken);

        if (request == null || request.Status != RequestStatus.Submitted)
            return false;

        request.Status = RequestStatus.Rejected;
        request.ApprovedByHRId = approverUserId; // Recorder of rejection
        request.RejectionReason = rejectionReason;
        request.ApprovedAt = DateTime.UtcNow; // Used as DecisionAt
        request.UpdatedAt = DateTime.UtcNow;

        _leaveRequestRepository.Update(request);
        await _leaveRequestRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<PaginatedResult<LeaveRequestDto>> GetLeaveRequestsAsync(
        int page, 
        int pageSize, 
        RequestStatus? status, 
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
         var query = _leaveRequestRepository.GetQueryable();
         
         // TODO: Include User if supported by Repository implementation, usually implies eager loading or projection
         
         if (userId.HasValue)
         {
             query = query.Where(r => r.UserId == userId.Value);
         }

         if (status.HasValue)
         {
             query = query.Where(r => r.Status == status.Value);
         }

         var totalCount = await query.CountAsync(cancellationToken);

         var items = await query
             .OrderByDescending(r => r.CreatedAt)
             .Skip((page - 1) * pageSize)
             .Take(pageSize)
             .Select(r => new LeaveRequestDto
             {
                 Id = r.Id,
                 UserId = r.UserId,
                 // UserName = r.User.FullName, // Assuming navigation property isn't easily available in simple projection without Include
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
             })
             .ToListAsync(cancellationToken);

         return new PaginatedResult<LeaveRequestDto>(items, totalCount, page, pageSize);
    }

    public async Task<LeaveRequestDto?> GetLeaveRequestByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var r = await _leaveRequestRepository.GetByIdAsync(id, cancellationToken);
        if (r == null) return null;

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
