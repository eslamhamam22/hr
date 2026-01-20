namespace HrSystem.Application.DTOs.Dashboard;

/// <summary>
/// Dashboard data for employees
/// </summary>
public class EmployeeDashboardDto
{
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    
    /// <summary>
    /// Leave summary for current month
    /// </summary>
    public LeaveSummaryDto LeaveSummary { get; set; } = new();
    
    /// <summary>
    /// Overtime summary for current month
    /// </summary>
    public OvertimeSummaryDto OvertimeSummary { get; set; } = new();
    
    /// <summary>
    /// Recent requests (last 5)
    /// </summary>
    public List<RecentRequestDto> RecentRequests { get; set; } = new();
}

/// <summary>
/// Dashboard data for managers
/// </summary>
public class ManagerDashboardDto : EmployeeDashboardDto
{
    /// <summary>
    /// Team leave summary for current month
    /// </summary>
    public List<TeamMemberSummaryDto> TeamSummary { get; set; } = new();
    
    /// <summary>
    /// Pending approvals count
    /// </summary>
    public int PendingApprovalsCount { get; set; }
    
    /// <summary>
    /// Leave requests by type for team
    /// </summary>
    public List<LeaveTypeCountDto> TeamLeaveByType { get; set; } = new();
    
    /// <summary>
    /// Monthly request trend for team
    /// </summary>
    public List<MonthlyTrendDto> MonthlyTrend { get; set; } = new();
}

public class LeaveSummaryDto
{
    public int TotalDaysUsed { get; set; }
    public int TotalDaysRemaining { get; set; }
    public int PendingRequests { get; set; }
    public int ApprovedRequests { get; set; }
    public int RejectedRequests { get; set; }
    public List<LeaveTypeCountDto> ByLeaveType { get; set; } = new();
}

public class OvertimeSummaryDto
{
    public decimal TotalHours { get; set; }
    public int TotalRequests { get; set; }
    public int PendingRequests { get; set; }
    public int ApprovedRequests { get; set; }
}

public class LeaveTypeCountDto
{
    public string LeaveType { get; set; } = string.Empty;
    public int Days { get; set; }
    public int Count { get; set; }
}

public class RecentRequestDto
{
    public Guid Id { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int? Days { get; set; }
    public decimal? Hours { get; set; }
}

public class TeamMemberSummaryDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int LeaveDays { get; set; }
    public decimal OvertimeHours { get; set; }
    public int PendingRequests { get; set; }
}

public class MonthlyTrendDto
{
    public string Month { get; set; } = string.Empty;
    public int LeaveRequests { get; set; }
    public int OvertimeRequests { get; set; }
}
