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
    private readonly IRepository<Department> _departmentRepository;
    private readonly IRepository<TimeOffRequest> _timeOffRequestRepository;
    private readonly IRepository<WorkFromHomeRequest> _workFromHomeRequestRepository;

    public ReportService(
        IRepository<LeaveRequest> leaveRequestRepository,
        IRepository<OvertimeRequest> overtimeRequestRepository,
        IRepository<AttendanceLog> attendanceRepository,
        IRepository<User> userRepository,
        IRepository<Department> departmentRepository,
        IRepository<TimeOffRequest> timeOffRequestRepository,
        IRepository<WorkFromHomeRequest> workFromHomeRequestRepository)
    {
        _leaveRequestRepository = leaveRequestRepository;
        _overtimeRequestRepository = overtimeRequestRepository;
        _attendanceRepository = attendanceRepository;
        _userRepository = userRepository;
        _departmentRepository = departmentRepository;
        _timeOffRequestRepository = timeOffRequestRepository;
        _workFromHomeRequestRepository = workFromHomeRequestRepository;
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

        var usersQuery = _userRepository.GetQueryable();
        if (departmentId.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.DepartmentId == departmentId.Value);
        }

        var joinedQuery = from a in query
                          join u in usersQuery on a.UserId equals u.Id
                          join d in _departmentRepository.GetQueryable() on u.DepartmentId equals d.Id into depts
                          from dept in depts.DefaultIfEmpty()
                          select new 
                          {
                              Employee = u.FullName,
                              Department = dept != null ? dept.Name : "N/A",
                              Date = a.Date,
                              CheckIn = a.CheckInTime,
                              CheckOut = a.CheckOutTime,
                              Hours = a.HoursWorked,
                              Late = a.IsLate ? "Yes" : "No",
                              a.Notes
                          };

        return await joinedQuery.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<dynamic>> GetLeaveSummaryReportAsync(
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var startOfYear = new DateTime(DateTime.UtcNow.Year, 1, 1);
        
        var query = _leaveRequestRepository.GetQueryable()
            .Where(r => r.CreatedAt >= startOfYear);
            
        var usersQuery = _userRepository.GetQueryable();
        if (departmentId.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.DepartmentId == departmentId.Value);
        }

        var rawData = from r in query
                      join u in usersQuery on r.UserId equals u.Id
                      join d in _departmentRepository.GetQueryable() on u.DepartmentId equals d.Id into depts
                      from dept in depts.DefaultIfEmpty()
                      select new { 
                          UserId = u.Id, 
                          Employee = u.FullName,
                          Department = dept != null ? dept.Name : "N/A",
                          r.LeaveType, 
                          r.TotalDays, 
                          r.Status 
                      };
                      
        var list = await rawData.ToListAsync(cancellationToken);
        
        var result = list
            .GroupBy(x => new { x.UserId, x.Employee, x.Department, x.LeaveType })
            .Select(g => new
            {
                g.Key.Employee,
                g.Key.Department,
                LeaveType = g.Key.LeaveType.ToString(),
                ApprovedDays = g.Where(x => x.Status == RequestStatus.Approved).Sum(x => x.TotalDays),
                PendingDays = g.Where(x => x.Status == RequestStatus.Submitted).Sum(x => x.TotalDays),
                TotalRequests = g.Count()
            })
            .OrderBy(x => x.Department).ThenBy(x => x.Employee);

        return result;
    }

    public async Task<IEnumerable<dynamic>> GetOvertimeAuditReportAsync(
        DateTime startDate,
        DateTime endDate,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _overtimeRequestRepository.GetQueryable()
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate);

        var usersQuery = _userRepository.GetQueryable();
        if (departmentId.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.DepartmentId == departmentId.Value);
        }

        var joinedQuery = from o in query
                          join u in usersQuery on o.UserId equals u.Id
                          join d in _departmentRepository.GetQueryable() on u.DepartmentId equals d.Id into depts
                          from dept in depts.DefaultIfEmpty()
                          orderby o.CreatedAt descending
                          select new
                          {
                              Employee = u.FullName,
                              Department = dept != null ? dept.Name : "N/A",
                              Date = o.StartDateTime.Date,
                              Start = o.StartDateTime,
                              End = o.EndDateTime,
                              Hours = o.HoursWorked,
                              Status = o.Status.ToString(),
                              Reason = o.Reason,
                              Submitted = o.CreatedAt
                          };

        return await joinedQuery.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<dynamic>> GetTimeOffReportAsync(
        DateTime startDate,
        DateTime endDate,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _timeOffRequestRepository.GetQueryable()
            .Where(r => r.Date >= startDate && r.Date <= endDate);

        var usersQuery = _userRepository.GetQueryable();
        if (departmentId.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.DepartmentId == departmentId.Value);
        }

        var joinedQuery = from r in query
                          join u in usersQuery on r.UserId equals u.Id
                          join d in _departmentRepository.GetQueryable() on u.DepartmentId equals d.Id into depts
                          from dept in depts.DefaultIfEmpty()
                          orderby r.Date descending
                          select new
                          {
                              Employee = u.FullName,
                              Department = dept != null ? dept.Name : "N/A",
                              Date = r.Date,
                              Time = r.StartTime,
                              Status = r.Status.ToString(),
                              Reason = r.Reason,
                              Submitted = r.SubmittedAt
                          };

        return await joinedQuery.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<dynamic>> GetWorkFromHomeReportAsync(
        DateTime startDate,
        DateTime endDate,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        // Filter requests that overlap with the date range
        var query = _workFromHomeRequestRepository.GetQueryable()
            .Where(r => r.FromDate <= endDate && r.ToDate >= startDate);

        var usersQuery = _userRepository.GetQueryable();
        if (departmentId.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.DepartmentId == departmentId.Value);
        }

        var joinedQuery = from r in query
                          join u in usersQuery on r.UserId equals u.Id
                          join d in _departmentRepository.GetQueryable() on u.DepartmentId equals d.Id into depts
                          from dept in depts.DefaultIfEmpty()
                          orderby r.FromDate descending
                          select new
                          {
                              Employee = u.FullName,
                              Department = dept != null ? dept.Name : "N/A",
                              From = r.FromDate,
                              To = r.ToDate,
                              Days = r.TotalDays,
                              Status = r.Status.ToString(),
                              Reason = r.Reason,
                              Submitted = r.SubmittedAt
                          };

        return await joinedQuery.ToListAsync(cancellationToken);
    }
}
