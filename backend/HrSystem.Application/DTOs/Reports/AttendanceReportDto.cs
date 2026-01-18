namespace HrSystem.Application.DTOs.Reports;

/// <summary>
/// DTO for attendance report
/// </summary>
public class AttendanceReportDto
{
    public Guid UserId { get; set; }
    
    public string EmployeeName { get; set; } = string.Empty;
    
    public string DepartmentName { get; set; } = string.Empty;
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    public decimal TotalHoursWorked { get; set; }
    
    public int TotalLates { get; set; }
    
    public int TotalAbsences { get; set; }
    
    public decimal AverageHoursPerDay { get; set; }
}
