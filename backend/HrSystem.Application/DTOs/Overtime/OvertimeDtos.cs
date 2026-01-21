using HrSystem.Domain.Enums;

namespace HrSystem.Application.DTOs.Overtime;

public class OvertimeRequestDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public decimal HoursWorked { get; set; }
    public string Reason { get; set; } = string.Empty;
    public RequestStatus Status { get; set; }
    public Guid? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public Guid? ApprovedByHRId { get; set; }
    public string? ApprovedByHRName { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public bool IsOverridden { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateOvertimeRequestDto
{
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public decimal HoursWorked { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class UpdateOvertimeRequestDto
{
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public decimal HoursWorked { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class RejectOvertimeRequestDto
{
    public string Reason { get; set; } = string.Empty;
}
