using HrSystem.Application.DTOs.Reports;

namespace HrSystem.Application.Services.Reports;

/// <summary>
/// Application service for reporting use cases
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Get monthly work summary report for all employees
    /// </summary>
    Task<IEnumerable<MonthlyWorkSummaryDto>> GetMonthlyWorkSummaryReportAsync(
        int year,
        int month,
        Guid? departmentId = null,
        Guid? employeeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get detailed monthly work report for a specific employee
    /// </summary>
    Task<MonthlyWorkDetailsDto?> GetMonthlyWorkDetailsReportAsync(
        Guid employeeId,
        int year,
        int month,
        CancellationToken cancellationToken = default);

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
