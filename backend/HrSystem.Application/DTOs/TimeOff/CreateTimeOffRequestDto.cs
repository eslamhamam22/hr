namespace HrSystem.Application.DTOs.TimeOff;

public class CreateTimeOffRequestDto
{
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public string Reason { get; set; } = string.Empty;
}
