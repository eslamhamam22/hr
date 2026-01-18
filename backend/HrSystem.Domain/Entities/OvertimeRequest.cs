using HrSystem.Domain.Common;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities;

/// <summary>
/// Overtime request entity
/// </summary>
public class OvertimeRequest : Entity, IAggregateRoot
{
    public Guid UserId { get; set; }
    
    public DateTime StartDateTime { get; set; }
    
    public DateTime EndDateTime { get; set; }
    
    public decimal HoursWorked { get; set; }
    
    public string Reason { get; set; } = string.Empty;
    
    public RequestStatus Status { get; set; } = RequestStatus.Draft;
    
    public Guid? ManagerId { get; set; }
    
    public Guid? ApprovedByHRId { get; set; }
    
    public DateTime? SubmittedAt { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
    
    public string? RejectionReason { get; set; }
    
    public bool IsOverridden { get; set; }
}
