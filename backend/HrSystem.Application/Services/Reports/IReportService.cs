namespace HrSystem.Application.Services.Reports;

/// <summary>
/// Application service for reporting use cases
/// </summary>
public interface IReportService
{
    Task<IEnumerable<dynamic>> GetAttendanceReportAsync(
        DateTime startDate, 
        DateTime endDate, 
        Guid? departmentId = null, 
        Guid? employeeId = null, 
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<dynamic>> GetLeaveSummaryReportAsync(
        Guid? departmentId = null, 
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<dynamic>> GetOvertimeAuditReportAsync(
        DateTime startDate, 
        DateTime endDate, 
        Guid? departmentId = null, 
        CancellationToken cancellationToken = default);

    Task<IEnumerable<dynamic>> GetTimeOffReportAsync(
        DateTime startDate,
        DateTime endDate,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<dynamic>> GetWorkFromHomeReportAsync(
        DateTime startDate,
        DateTime endDate,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default);
}
