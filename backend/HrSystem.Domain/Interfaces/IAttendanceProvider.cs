namespace HrSystem.Domain.Interfaces;

/// <summary>
/// Abstraction for external attendance data provider
/// </summary>
public interface IAttendanceProvider
{
    Task<IEnumerable<AttendanceLogData>> FetchAttendanceLogsAsync(
        DateTime startDate, 
        DateTime endDate, 
        CancellationToken cancellationToken = default);
}

public class AttendanceLogData
{
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan CheckIn { get; set; }
    public TimeSpan? CheckOut { get; set; }
    public decimal HoursWorked { get; set; }
}
