namespace HrSystem.Application.DTOs.Reports;

/// <summary>
/// Summary report showing total work hours per employee per month
/// </summary>
public class MonthlyWorkSummaryDto
{
    public Guid UserId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public int WorkingDaysInMonth { get; set; }
    public decimal ExpectedHours { get; set; }
    public decimal ActualAttendanceHours { get; set; }
    public decimal LeaveHours { get; set; }
    public decimal WorkFromHomeHours { get; set; }
    public decimal TimeOffHours { get; set; }
    public decimal TotalHoursWorked { get; set; }
    public int DaysPresent { get; set; }
    public int DaysOnLeave { get; set; }
    public int DaysWorkFromHome { get; set; }
    public int DaysAbsent { get; set; }
}

/// <summary>
/// Daily detail record for the detailed report
/// </summary>
public class DailyWorkDetailDto
{
    public DateTime Date { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public bool IsWeekend { get; set; }
    public decimal AttendanceHours { get; set; }
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public string Status { get; set; } = string.Empty; // Present, Leave, WFH, Absent, Weekend
    public string? LeaveType { get; set; }
    public decimal TimeOffHours { get; set; }
    public decimal TotalHours { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Detailed monthly report for a single employee
/// </summary>
public class MonthlyWorkDetailsDto
{
    public Guid UserId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public List<DailyWorkDetailDto> DailyDetails { get; set; } = new();
    
    // Totals
    public int WorkingDaysInMonth { get; set; }
    public decimal ExpectedHours { get; set; }
    public decimal TotalAttendanceHours { get; set; }
    public decimal TotalLeaveHours { get; set; }
    public decimal TotalWorkFromHomeHours { get; set; }
    public decimal TotalTimeOffHours { get; set; }
    public decimal GrandTotalHours { get; set; }
    public int TotalDaysPresent { get; set; }
    public int TotalDaysOnLeave { get; set; }
    public int TotalDaysWorkFromHome { get; set; }
    public int TotalDaysAbsent { get; set; }
}
