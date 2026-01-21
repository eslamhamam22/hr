namespace HrSystem.Application.DTOs.Requests;

/// <summary>
/// DTO for displaying request summary
/// </summary>
public class RequestSummaryDto
{
    public Guid Id { get; set; }
    
    public string EmployeeName { get; set; } = string.Empty;
    
    public string RequestType { get; set; } = string.Empty;
    
    public string Status { get; set; } = string.Empty;
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    public DateTime SubmittedAt { get; set; }
    
    public string? ApprovedByName { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
    
    public Guid? UserId { get; set; }
    
    public int? DepartmentId { get; set; }
}
