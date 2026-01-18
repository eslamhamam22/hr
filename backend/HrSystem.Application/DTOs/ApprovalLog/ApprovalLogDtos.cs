namespace HrSystem.Application.DTOs.ApprovalLog;

public class ApprovalLogDto
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public Guid ApprovedByUserId { get; set; }
    public string? ApprovedByUserName { get; set; }
    public bool Approved { get; set; }
    public string? Comments { get; set; }
    public bool IsOverride { get; set; }
    public string? OverrideReason { get; set; }
    public DateTime CreatedAt { get; set; }
}
