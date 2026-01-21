using HrSystem.Domain.Common;
using HrSystem.Domain.Enums;

namespace HrSystem.Domain.Entities;

/// <summary>
/// Work From Home request entity
/// </summary>
public class WorkFromHomeRequest : Entity, IAggregateRoot
{
    public Guid UserId { get; set; }
    
    public DateTime FromDate { get; set; }
    
    public DateTime ToDate { get; set; }
    
    public int TotalDays { get; set; }
    
    public string Reason { get; set; } = string.Empty;
    
    public RequestStatus Status { get; set; } = RequestStatus.Draft;
    
    public Guid? ManagerId { get; set; }
    
    public Guid? ApprovedByHRId { get; set; }
    
    public DateTime? SubmittedAt { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
    
    public string? RejectionReason { get; set; }
}
