using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities;

/// <summary>
/// Daily attendance log from external provider
/// </summary>
public class AttendanceLog : Entity
{
    public Guid UserId { get; set; }
    
    public DateTime Date { get; set; }
    
    public TimeSpan CheckInTime { get; set; }
    
    public TimeSpan? CheckOutTime { get; set; }
    
    public decimal HoursWorked { get; set; }
    
    public bool IsLate { get; set; }
    
    public bool IsAbsent { get; set; }
    
    public string? Notes { get; set; }
}
