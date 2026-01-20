using HrSystem.Application.DTOs.Dashboard;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using HrSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Services.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly IRepository<LeaveRequest> _leaveRequestRepository;
    private readonly IRepository<OvertimeRequest> _overtimeRequestRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Department> _departmentRepository;

    public DashboardService(
        IRepository<LeaveRequest> leaveRequestRepository,
        IRepository<OvertimeRequest> overtimeRequestRepository,
        IRepository<User> userRepository,
        IRepository<Department> departmentRepository)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _overtimeRequestRepository = overtimeRequestRepository;
        _userRepository = userRepository;
        _departmentRepository = departmentRepository;
    }

    public async Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            return new EmployeeDashboardDto();

        var department = user.DepartmentId.HasValue 
            ? await _departmentRepository.GetByIdAsync(user.DepartmentId.Value, cancellationToken)
            : null;

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
        var startOfYear = new DateTime(now.Year, 1, 1);

        // Get leave requests for current year
        var leaveRequests = await _leaveRequestRepository.GetQueryable()
            .Where(r => r.UserId == userId && r.CreatedAt >= startOfYear)
            .ToListAsync(cancellationToken);

        // Get overtime requests for current month
        var overtimeRequests = await _overtimeRequestRepository.GetQueryable()
            .Where(r => r.UserId == userId && r.CreatedAt >= startOfMonth)
            .ToListAsync(cancellationToken);

        // Get recent requests (last 5)
        var recentLeave = await _leaveRequestRepository.GetQueryable()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        var recentOvertime = await _overtimeRequestRepository.GetQueryable()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        var recentRequests = new List<RecentRequestDto>();
        
        recentRequests.AddRange(recentLeave.Select(r => new RecentRequestDto
        {
            Id = r.Id,
            RequestType = "Leave - " + r.LeaveType.ToString(),
            Status = r.Status.ToString(),
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            SubmittedAt = r.SubmittedAt ?? r.CreatedAt,
            Days = r.TotalDays
        }));

        recentRequests.AddRange(recentOvertime.Select(r => new RecentRequestDto
        {
            Id = r.Id,
            RequestType = "Overtime",
            Status = r.Status.ToString(),
            StartDate = r.StartDateTime,
            EndDate = r.EndDateTime,
            SubmittedAt = r.SubmittedAt ?? r.CreatedAt,
            Hours = r.HoursWorked
        }));

        // Leave summary
        var leaveSummary = new LeaveSummaryDto
        {
            TotalDaysUsed = leaveRequests
                .Where(r => r.Status == RequestStatus.Approved)
                .Sum(r => r.TotalDays),
            TotalDaysRemaining = 21 - leaveRequests
                .Where(r => r.Status == RequestStatus.Approved)
                .Sum(r => r.TotalDays), // Assuming 21 days annual leave
            PendingRequests = leaveRequests.Count(r => r.Status == RequestStatus.Submitted),
            ApprovedRequests = leaveRequests.Count(r => r.Status == RequestStatus.Approved),
            RejectedRequests = leaveRequests.Count(r => r.Status == RequestStatus.Rejected),
            ByLeaveType = leaveRequests
                .Where(r => r.Status == RequestStatus.Approved)
                .GroupBy(r => r.LeaveType)
                .Select(g => new LeaveTypeCountDto
                {
                    LeaveType = g.Key.ToString(),
                    Days = g.Sum(r => r.TotalDays),
                    Count = g.Count()
                })
                .ToList()
        };

        // Overtime summary for current month
        var overtimeSummary = new OvertimeSummaryDto
        {
            TotalHours = overtimeRequests
                .Where(r => r.Status == RequestStatus.Approved)
                .Sum(r => r.HoursWorked),
            TotalRequests = overtimeRequests.Count,
            PendingRequests = overtimeRequests.Count(r => r.Status == RequestStatus.Submitted),
            ApprovedRequests = overtimeRequests.Count(r => r.Status == RequestStatus.Approved)
        };

        return new EmployeeDashboardDto
        {
            EmployeeName = user.FullName,
            DepartmentName = department?.Name ?? "Not Assigned",
            Role = user.Role.ToString(),
            LeaveSummary = leaveSummary,
            OvertimeSummary = overtimeSummary,
            RecentRequests = recentRequests
                .OrderByDescending(r => r.SubmittedAt)
                .Take(5)
                .ToList()
        };
    }

    public async Task<ManagerDashboardDto> GetManagerDashboardAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            return new ManagerDashboardDto();

        // Get base employee dashboard data
        var employeeDashboard = await GetEmployeeDashboardAsync(userId, cancellationToken);
        
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        // Get team members (users who have this manager)
        var teamMembers = await _userRepository.GetQueryable()
            .Where(u => u.ManagerId == userId && u.IsActive)
            .ToListAsync(cancellationToken);

        var teamMemberIds = teamMembers.Select(u => u.Id).ToList();

        // Get pending approvals count
        var pendingLeave = await _leaveRequestRepository.GetQueryable()
            .Where(r => teamMemberIds.Contains(r.UserId) && r.Status == RequestStatus.Submitted)
            .CountAsync(cancellationToken);

        var pendingOvertime = await _overtimeRequestRepository.GetQueryable()
            .Where(r => teamMemberIds.Contains(r.UserId) && r.Status == RequestStatus.Submitted)
            .CountAsync(cancellationToken);

        // Team summary
        var teamLeaveRequests = await _leaveRequestRepository.GetQueryable()
            .Where(r => teamMemberIds.Contains(r.UserId) && r.CreatedAt >= startOfMonth)
            .ToListAsync(cancellationToken);

        var teamOvertimeRequests = await _overtimeRequestRepository.GetQueryable()
            .Where(r => teamMemberIds.Contains(r.UserId) && r.CreatedAt >= startOfMonth)
            .ToListAsync(cancellationToken);

        var teamSummary = teamMembers.Select(member => new TeamMemberSummaryDto
        {
            UserId = member.Id,
            UserName = member.FullName,
            LeaveDays = teamLeaveRequests
                .Where(r => r.UserId == member.Id && r.Status == RequestStatus.Approved)
                .Sum(r => r.TotalDays),
            OvertimeHours = teamOvertimeRequests
                .Where(r => r.UserId == member.Id && r.Status == RequestStatus.Approved)
                .Sum(r => r.HoursWorked),
            PendingRequests = teamLeaveRequests.Count(r => r.UserId == member.Id && r.Status == RequestStatus.Submitted) +
                              teamOvertimeRequests.Count(r => r.UserId == member.Id && r.Status == RequestStatus.Submitted)
        }).ToList();

        // Leave by type for team
        var teamLeaveByType = teamLeaveRequests
            .Where(r => r.Status == RequestStatus.Approved)
            .GroupBy(r => r.LeaveType)
            .Select(g => new LeaveTypeCountDto
            {
                LeaveType = g.Key.ToString(),
                Days = g.Sum(r => r.TotalDays),
                Count = g.Count()
            })
            .ToList();

        // Monthly trend (last 6 months)
        var monthlyTrend = new List<MonthlyTrendDto>();
        for (int i = 5; i >= 0; i--)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            
            var leaveCount = await _leaveRequestRepository.GetQueryable()
                .Where(r => teamMemberIds.Contains(r.UserId) && 
                            r.CreatedAt >= monthStart && r.CreatedAt <= monthEnd)
                .CountAsync(cancellationToken);

            var overtimeCount = await _overtimeRequestRepository.GetQueryable()
                .Where(r => teamMemberIds.Contains(r.UserId) && 
                            r.CreatedAt >= monthStart && r.CreatedAt <= monthEnd)
                .CountAsync(cancellationToken);

            monthlyTrend.Add(new MonthlyTrendDto
            {
                Month = monthStart.ToString("MMM yyyy"),
                LeaveRequests = leaveCount,
                OvertimeRequests = overtimeCount
            });
        }

        return new ManagerDashboardDto
        {
            EmployeeName = employeeDashboard.EmployeeName,
            DepartmentName = employeeDashboard.DepartmentName,
            Role = employeeDashboard.Role,
            LeaveSummary = employeeDashboard.LeaveSummary,
            OvertimeSummary = employeeDashboard.OvertimeSummary,
            RecentRequests = employeeDashboard.RecentRequests,
            TeamSummary = teamSummary,
            PendingApprovalsCount = pendingLeave + pendingOvertime,
            TeamLeaveByType = teamLeaveByType,
            MonthlyTrend = monthlyTrend
        };
    }
}
