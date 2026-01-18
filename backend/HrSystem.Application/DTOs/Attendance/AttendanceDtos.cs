namespace HrSystem.Application.DTOs.Attendance;

public class AttendanceLogDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime Date { get; set; }
    public string CheckInTime { get; set; } = string.Empty;
    public string? CheckOutTime { get; set; }
    public decimal HoursWorked { get; set; }
    public bool IsLate { get; set; }
    public bool IsAbsent { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
