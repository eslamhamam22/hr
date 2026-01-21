using HrSystem.Domain.Enums;

namespace HrSystem.Application.DTOs.TimeOff;

public class TimeOffRequestDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public RequestStatus Status { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByHRName { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
}
