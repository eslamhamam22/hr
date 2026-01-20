using HrSystem.Domain.Entities;
using HrSystem.Domain.Enums;
using HrSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Services.Reports;

public class ReportService : IReportService
{
    private readonly IRepository<LeaveRequest> _leaveRequestRepository;
    private readonly IRepository<OvertimeRequest> _overtimeRequestRepository;
    private readonly IRepository<AttendanceLog> _attendanceRepository;
    private readonly IRepository<User> _userRepository;

    public ReportService(
        IRepository<LeaveRequest> leaveRequestRepository,
        IRepository<OvertimeRequest> overtimeRequestRepository,
        IRepository<AttendanceLog> attendanceRepository,
        IRepository<User> userRepository)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _overtimeRequestRepository = overtimeRequestRepository;
        _attendanceRepository = attendanceRepository;
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<dynamic>> GetAttendanceReportAsync(
        DateTime startDate,
        DateTime endDate,
        Guid? departmentId = null,
        Guid? employeeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _attendanceRepository.GetQueryable()
            .Where(a => a.Date >= startDate && a.Date <= endDate);

        if (employeeId.HasValue)
        {
            query = query.Where(a => a.UserId == employeeId.Value);
        }

        var attendanceRecords = await query
            .OrderByDescending(a => a.Date)
            .Select(a => new
            {
                a.Id,
                a.UserId,
                a.Date,
                a.CheckInTime,
                a.CheckOutTime,
                a.HoursWorked,
                a.IsLate,
                a.Notes
            })
            .ToListAsync(cancellationToken);

        return attendanceRecords.Cast<dynamic>();
    }

    public async Task<IEnumerable<dynamic>> GetLeaveSummaryReportAsync(
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _leaveRequestRepository.GetQueryable();

        var startOfYear = new DateTime(DateTime.UtcNow.Year, 1, 1);

        var leaveRequests = await query
            .Where(r => r.CreatedAt >= startOfYear)
            .GroupBy(r => new { r.UserId, r.LeaveType })
            .Select(g => new
            {
                UserId = g.Key.UserId,
                LeaveType = g.Key.LeaveType.ToString(),
                TotalDays = g.Where(r => r.Status == RequestStatus.Approved).Sum(r => r.TotalDays),
                PendingDays = g.Where(r => r.Status == RequestStatus.Submitted).Sum(r => r.TotalDays),
                RequestCount = g.Count()
            })
            .ToListAsync(cancellationToken);

        return leaveRequests.Cast<dynamic>();
    }

    public async Task<IEnumerable<dynamic>> GetOvertimeAuditReportAsync(
        DateTime startDate,
        DateTime endDate,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _overtimeRequestRepository.GetQueryable()
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate);

        var overtimeRecords = await query
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new
            {
                o.Id,
                o.UserId,
                o.StartDateTime,
                o.EndDateTime,
                o.HoursWorked,
                Status = o.Status.ToString(),
                o.Reason,
                o.ApprovedAt,
                o.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return overtimeRecords.Cast<dynamic>();
    }
}
