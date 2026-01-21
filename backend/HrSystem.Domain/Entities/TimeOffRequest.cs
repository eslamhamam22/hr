using HrSystem.Domain.Common;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities;

/// <summary>
/// Time Off request entity (2 hours max, once per month)
/// </summary>
public class TimeOffRequest : Entity, IAggregateRoot
{
    public Guid UserId { get; set; }
    
    public DateTime Date { get; set; }
    
    public TimeSpan StartTime { get; set; }
    
    // Duration is fixed to 2 hours usually, but we can store it or imply it. 
    // User said "this request is 2 hours".
    
    public string Reason { get; set; } = string.Empty;
    
    public RequestStatus Status { get; set; } = RequestStatus.Draft;
    
    public Guid? ManagerId { get; set; }
    
    public Guid? ApprovedByHRId { get; set; }
    
    public DateTime? SubmittedAt { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
    
    public string? RejectionReason { get; set; }
    
    public bool IsOverridden { get; set; }
}
