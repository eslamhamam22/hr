using HrSystem.Domain.Enums;

namespace HrSystem.Application.DTOs.WorkFromHome;

public class WorkFromHomeRequestDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public RequestStatus Status { get; set; }
    public Guid? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public Guid? ApprovedByHRId { get; set; }
    public string? ApprovedByHRName { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateWorkFromHomeRequestDto
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string Reason { get; set; } = string.Empty;
}
