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
    private readonly IRepository<WorkFromHomeRequest> _workFromHomeRequestRepository;
    private readonly IRepository<TimeOffRequest> _timeOffRequestRepository;

    public DashboardService(
        IRepository<LeaveRequest> leaveRequestRepository,
        IRepository<OvertimeRequest> overtimeRequestRepository,
        IRepository<User> userRepository,
        IRepository<Department> departmentRepository,
        IRepository<WorkFromHomeRequest> workFromHomeRequestRepository,
        IRepository<TimeOffRequest> timeOffRequestRepository)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _overtimeRequestRepository = overtimeRequestRepository;
        _userRepository = userRepository;
        _departmentRepository = departmentRepository;
        _workFromHomeRequestRepository = workFromHomeRequestRepository;
        _timeOffRequestRepository = timeOffRequestRepository;
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

        // Get work from home requests for current month
        var workFromHomeRequests = await _workFromHomeRequestRepository.GetQueryable()
            .Where(r => r.UserId == userId && r.CreatedAt >= startOfYear)
            .ToListAsync(cancellationToken);

        // Get time off requests for current month
        var timeOffRequests = await _timeOffRequestRepository.GetQueryable()
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

        var recentWorkFromHome = await _workFromHomeRequestRepository.GetQueryable()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        var recentTimeOff = await _timeOffRequestRepository.GetQueryable()
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

        recentRequests.AddRange(recentWorkFromHome.Select(r => new RecentRequestDto
        {
            Id = r.Id,
            RequestType = "Work From Home",
            Status = r.Status.ToString(),
            StartDate = r.FromDate,
            EndDate = r.ToDate,
            SubmittedAt = r.SubmittedAt ?? r.CreatedAt,
            Days = r.TotalDays
        }));

        recentRequests.AddRange(recentTimeOff.Select(r => new RecentRequestDto
        {
            Id = r.Id,
            RequestType = "Time Off",
            Status = r.Status.ToString(),
            StartDate = r.Date,
            EndDate = r.Date, 
            SubmittedAt = r.SubmittedAt ?? r.CreatedAt,
            Hours = 2 
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
            PendingRequests = leaveRequests.Count(r => r.Status == RequestStatus.Submitted || r.Status == RequestStatus.PendingManager || r.Status == RequestStatus.PendingHR),
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
            PendingRequests = overtimeRequests.Count(r => r.Status == RequestStatus.Submitted || r.Status == RequestStatus.PendingManager || r.Status == RequestStatus.PendingHR),
            ApprovedRequests = overtimeRequests.Count(r => r.Status == RequestStatus.Approved)
        };

        // Work From Home summary for current month
        var workFromHomeSummary = new WorkFromHomeSummaryDto
        {
            TotalDays = workFromHomeRequests
                .Where(r => r.Status == RequestStatus.Approved)
                .Sum(r => r.TotalDays),
            TotalRequests = workFromHomeRequests.Count,
            PendingRequests = workFromHomeRequests.Count(r => r.Status == RequestStatus.Submitted || r.Status == RequestStatus.PendingManager || r.Status == RequestStatus.PendingHR),
            ApprovedRequests = workFromHomeRequests.Count(r => r.Status == RequestStatus.Approved)
        };

        // Time Off summary for current month
        var timeOffSummary = new TimeOffSummaryDto
        {
            TotalRequests = timeOffRequests.Count,
            PendingRequests = timeOffRequests.Count(r => r.Status == RequestStatus.Submitted || r.Status == RequestStatus.PendingManager || r.Status == RequestStatus.PendingHR),
            ApprovedRequests = timeOffRequests.Count(r => r.Status == RequestStatus.Approved),
            RejectedRequests = timeOffRequests.Count(r => r.Status == RequestStatus.Rejected)
        };

        return new EmployeeDashboardDto
        {
            EmployeeName = user.FullName,
            DepartmentName = department?.Name ?? "Not Assigned",
            Role = user.Role.ToString(),
            LeaveSummary = leaveSummary,
            OvertimeSummary = overtimeSummary,
            WorkFromHomeSummary = workFromHomeSummary,
            TimeOffSummary = timeOffSummary,
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
            .Where(r => teamMemberIds.Contains(r.UserId) && (r.Status == RequestStatus.Submitted || r.Status == RequestStatus.PendingManager))
            .CountAsync(cancellationToken);

        var pendingOvertime = await _overtimeRequestRepository.GetQueryable()
            .Where(r => teamMemberIds.Contains(r.UserId) && (r.Status == RequestStatus.Submitted || r.Status == RequestStatus.PendingManager))
            .CountAsync(cancellationToken);

        // Team summary
        var teamLeaveRequests = await _leaveRequestRepository.GetQueryable()
            .Where(r => teamMemberIds.Contains(r.UserId) && r.CreatedAt >= startOfMonth)
            .ToListAsync(cancellationToken);

        var teamOvertimeRequests = await _overtimeRequestRepository.GetQueryable()
            .Where(r => teamMemberIds.Contains(r.UserId) && r.CreatedAt >= startOfMonth)
            .ToListAsync(cancellationToken);

        var teamWorkFromHomeRequests = await _workFromHomeRequestRepository.GetQueryable()
            .Where(r => teamMemberIds.Contains(r.UserId) && r.CreatedAt >= startOfMonth)
            .ToListAsync(cancellationToken);

        var teamTimeOffRequests = await _timeOffRequestRepository.GetQueryable()
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
            PendingRequests = teamLeaveRequests.Count(r => r.UserId == member.Id && (r.Status == RequestStatus.Submitted || r.Status == RequestStatus.PendingManager)) +
                              teamOvertimeRequests.Count(r => r.UserId == member.Id && (r.Status == RequestStatus.Submitted || r.Status == RequestStatus.PendingManager)) +
                              teamWorkFromHomeRequests.Count(r => r.UserId == member.Id && (r.Status == RequestStatus.Submitted || r.Status == RequestStatus.PendingManager)),
            WorkFromHomeDays = teamWorkFromHomeRequests
                .Where(r => r.UserId == member.Id && r.Status == RequestStatus.Approved)
                .Sum(r => r.TotalDays),
            TimeOffCount = teamTimeOffRequests
                .Count(r => r.UserId == member.Id && r.Status == RequestStatus.Approved)
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

            var workFromHomeCount = await _workFromHomeRequestRepository.GetQueryable()
                .Where(r => teamMemberIds.Contains(r.UserId) && 
                            r.CreatedAt >= monthStart && r.CreatedAt <= monthEnd)
                .CountAsync(cancellationToken);

            var timeOffCount = await _timeOffRequestRepository.GetQueryable()
                .Where(r => teamMemberIds.Contains(r.UserId) && 
                            r.CreatedAt >= monthStart && r.CreatedAt <= monthEnd)
                .CountAsync(cancellationToken);

            monthlyTrend.Add(new MonthlyTrendDto
            {
                Month = monthStart.ToString("MMM yyyy"),
                LeaveRequests = leaveCount,
                OvertimeRequests = overtimeCount,
                WorkFromHomeRequests = workFromHomeCount,
                TimeOffRequests = timeOffCount
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

    public async Task<AdminDashboardDto> GetAdminDashboardAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Get base employee dashboard data for the admin themselves
        var employeeDashboard = await GetEmployeeDashboardAsync(userId, cancellationToken);
        
        var now = DateTime.UtcNow;
        var startOfYear = new DateTime(now.Year, 1, 1);
        
        // System Stats
        var totalEmployees = await _userRepository.GetQueryable().CountAsync(cancellationToken);
        var totalDepartments = await _departmentRepository.GetQueryable().CountAsync(cancellationToken);
        
        // System Leave Stats (Current Year)
        var allLeaveRequests = await _leaveRequestRepository.GetQueryable()
            .Where(r => r.CreatedAt >= startOfYear)
            .ToListAsync(cancellationToken);
            
        var systemLeaveStats = new RequestStatsDto
        {
            Total = allLeaveRequests.Count,
            Pending = allLeaveRequests.Count(r => r.Status == RequestStatus.Submitted || r.Status == RequestStatus.PendingManager || r.Status == RequestStatus.PendingHR),
            Approved = allLeaveRequests.Count(r => r.Status == RequestStatus.Approved),
            Rejected = allLeaveRequests.Count(r => r.Status == RequestStatus.Rejected)
        };
        
        // System Overtime Stats (Current Year)
        var allOvertimeRequests = await _overtimeRequestRepository.GetQueryable()
            .Where(r => r.CreatedAt >= startOfYear)
            .ToListAsync(cancellationToken);
            
        // System Work From Home Stats (Current Year)
        var allWorkFromHomeRequests = await _workFromHomeRequestRepository.GetQueryable()
            .Where(r => r.CreatedAt >= startOfYear)
            .ToListAsync(cancellationToken);

        // System Time Off Stats (Current Year)
        var allTimeOffRequests = await _timeOffRequestRepository.GetQueryable()
            .Where(r => r.CreatedAt >= startOfYear)
            .ToListAsync(cancellationToken);
            
        var systemOvertimeStats = new RequestStatsDto
        {
            Total = allOvertimeRequests.Count,
            Pending = allOvertimeRequests.Count(r => r.Status == RequestStatus.Submitted || r.Status == RequestStatus.PendingManager || r.Status == RequestStatus.PendingHR),
            Approved = allOvertimeRequests.Count(r => r.Status == RequestStatus.Approved),
            Rejected = allOvertimeRequests.Count(r => r.Status == RequestStatus.Rejected)
        };

        var systemWorkFromHomeStats = new RequestStatsDto
        {
            Total = allWorkFromHomeRequests.Count,
            Pending = allWorkFromHomeRequests.Count(r => r.Status == RequestStatus.Submitted || r.Status == RequestStatus.PendingManager || r.Status == RequestStatus.PendingHR),
            Approved = allWorkFromHomeRequests.Count(r => r.Status == RequestStatus.Approved),
            Rejected = allWorkFromHomeRequests.Count(r => r.Status == RequestStatus.Rejected)
        };

        var systemTimeOffStats = new RequestStatsDto
        {
            Total = allTimeOffRequests.Count,
            Pending = allTimeOffRequests.Count(r => r.Status == RequestStatus.Submitted || r.Status == RequestStatus.PendingManager || r.Status == RequestStatus.PendingHR),
            Approved = allTimeOffRequests.Count(r => r.Status == RequestStatus.Approved),
            Rejected = allTimeOffRequests.Count(r => r.Status == RequestStatus.Rejected)
        };
        
        // Requests by Department
        var departments = await _departmentRepository.GetQueryable().ToListAsync(cancellationToken);
        
        // Get user department mapping
        var userDepartmentMap = await _userRepository.GetQueryable()
            .Where(u => u.DepartmentId.HasValue)
            .ToDictionaryAsync(u => u.Id, u => u.DepartmentId.Value, cancellationToken);
            
        var requestsByDepartment = departments.Select(d => 
        {
            var deptLeaveCount = allLeaveRequests.Count(r => userDepartmentMap.ContainsKey(r.UserId) && userDepartmentMap[r.UserId] == d.Id);
            var deptOvertimeCount = allOvertimeRequests.Count(r => userDepartmentMap.ContainsKey(r.UserId) && userDepartmentMap[r.UserId] == d.Id);
            var deptWorkFromHomeCount = allWorkFromHomeRequests.Count(r => userDepartmentMap.ContainsKey(r.UserId) && userDepartmentMap[r.UserId] == d.Id);
            var deptTimeOffCount = allTimeOffRequests.Count(r => userDepartmentMap.ContainsKey(r.UserId) && userDepartmentMap[r.UserId] == d.Id);
            
            return new DepartmentRequestCountDto
            {
                DepartmentName = d.Name,
                LeaveRequests = deptLeaveCount,
                OvertimeRequests = deptOvertimeCount,
                WorkFromHomeRequests = deptWorkFromHomeCount,
                TimeOffRequests = deptTimeOffCount
            };
        }).ToList();
        
        // System Monthly Trend (Last 6 Months)
        var monthlyTrend = new List<MonthlyTrendDto>();
        for (int i = 5; i >= 0; i--)
        {
            var monthStart = new DateTime(now.Year, now.Month, 1).AddMonths(-i);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            
            var leaveCount = await _leaveRequestRepository.GetQueryable()
                .Where(r => r.CreatedAt >= monthStart && r.CreatedAt <= monthEnd)
                .CountAsync(cancellationToken);

            var overtimeCount = await _overtimeRequestRepository.GetQueryable()
                .Where(r => r.CreatedAt >= monthStart && r.CreatedAt <= monthEnd)
                .CountAsync(cancellationToken);

            var workFromHomeCount = await _workFromHomeRequestRepository.GetQueryable()
                .Where(r => r.CreatedAt >= monthStart && r.CreatedAt <= monthEnd)
                .CountAsync(cancellationToken);

            var timeOffCount = await _timeOffRequestRepository.GetQueryable()
                .Where(r => r.CreatedAt >= monthStart && r.CreatedAt <= monthEnd)
                .CountAsync(cancellationToken);

            monthlyTrend.Add(new MonthlyTrendDto
            {
                Month = monthStart.ToString("MMM yyyy"),
                LeaveRequests = leaveCount,
                OvertimeRequests = overtimeCount,
                WorkFromHomeRequests = workFromHomeCount,
                TimeOffRequests = timeOffCount
            });
        }
        
        // System Leave Type Distribution
        var systemLeaveTypeDistribution = allLeaveRequests
            .Where(r => r.Status == RequestStatus.Approved)
            .GroupBy(r => r.LeaveType)
            .Select(g => new LeaveTypeCountDto
            {
                LeaveType = g.Key.ToString(),
                Days = g.Sum(r => r.TotalDays),
                Count = g.Count()
            })
            .ToList();

        return new AdminDashboardDto
        {
            EmployeeName = employeeDashboard.EmployeeName,
            DepartmentName = employeeDashboard.DepartmentName,
            Role = employeeDashboard.Role,
            LeaveSummary = employeeDashboard.LeaveSummary,
            OvertimeSummary = employeeDashboard.OvertimeSummary,
            TimeOffSummary = employeeDashboard.TimeOffSummary,
            WorkFromHomeSummary = employeeDashboard.WorkFromHomeSummary,
            RecentRequests = employeeDashboard.RecentRequests,
            
            TotalEmployees = totalEmployees,
            TotalDepartments = totalDepartments,
            SystemLeaveStats = systemLeaveStats,
            SystemOvertimeStats = systemOvertimeStats,
            SystemWorkFromHomeStats = systemWorkFromHomeStats,
            SystemTimeOffStats = systemTimeOffStats,
            RequestsByDepartment = requestsByDepartment,
            SystemMonthlyTrend = monthlyTrend,
            SystemLeaveTypeDistribution = systemLeaveTypeDistribution
        };
    }
}
