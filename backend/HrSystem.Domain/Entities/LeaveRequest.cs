using HrSystem.Domain.Common;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities;

/// <summary>
/// Leave request entity with business logic
/// </summary>
public class LeaveRequest : Entity, IAggregateRoot
{
    public Guid UserId { get; set; }
    
    public LeaveType LeaveType { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    public int TotalDays { get; set; }
    
    public string Reason { get; set; } = string.Empty;
    
    public RequestStatus Status { get; set; } = RequestStatus.Draft;
    
    public Guid? ManagerId { get; set; }
    
    public Guid? ApprovedByHRId { get; set; }
    
    public DateTime? SubmittedAt { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
    
    public string? RejectionReason { get; set; }
    
    public bool IsOverridden { get; set; }
    
    public string? OverrideReason { get; set; }
}
