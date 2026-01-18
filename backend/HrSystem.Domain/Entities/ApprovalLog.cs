using HrSystem.Domain.Common;

namespace HrSystem.Domain.Entities;

/// <summary>
/// Audit trail for approvals
/// </summary>
public class ApprovalLog : Entity
{
    public Guid RequestId { get; set; }
    
    public string RequestType { get; set; } = string.Empty; // "LeaveRequest" or "OvertimeRequest"
    
    public Guid ApprovedByUserId { get; set; }
    
    public bool Approved { get; set; }
    
    public string? Comments { get; set; }
    
    public bool IsOverride { get; set; }
    
    public string? OverrideReason { get; set; }
}
