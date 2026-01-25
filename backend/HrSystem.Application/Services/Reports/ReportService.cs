using HrSystem.Application.DTOs.Reports;
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

    // Constants for work calculations - will be configurable via settings in future
    private const decimal StandardWorkHoursPerDay = 8m;
    private const decimal TimeOffHoursPerRequest = 2m;

    /// <summary>
    /// Check if a date is a weekend (Friday or Saturday)
    /// </summary>
    private static bool IsWeekend(DateTime date)
    {
        return date.DayOfWeek == System.DayOfWeek.Friday || date.DayOfWeek == System.DayOfWeek.Saturday;
    }

    /// <summary>
    /// Get all working days in a month (excluding weekends)
    /// </summary>
    private static List<DateTime> GetWorkingDaysInMonth(int year, int month)
    {
        var workingDays = new List<DateTime>();
        var daysInMonth = DateTime.DaysInMonth(year, month);
        
        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(year, month, day);
            if (!IsWeekend(date))
            {
                workingDays.Add(date);
            }
        }
        
        return workingDays;
    }

    public async Task<IEnumerable<MonthlyWorkSummaryDto>> GetMonthlyWorkSummaryReportAsync(
        int year,
        int month,
        Guid? departmentId = null,
        Guid? employeeId = null,
        CancellationToken cancellationToken = default)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        var workingDays = GetWorkingDaysInMonth(year, month);
        var workingDaysCount = workingDays.Count;
        var expectedHours = workingDaysCount * StandardWorkHoursPerDay;

        // Get users filtered by department and/or employee
        var usersQuery = _userRepository.GetQueryable().Where(u => u.IsActive);
        if (departmentId.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.DepartmentId == departmentId.Value);
        }
        if (employeeId.HasValue)
        {
            usersQuery = usersQuery.Where(u => u.Id == employeeId.Value);
        }

        var users = await usersQuery.ToListAsync(cancellationToken);
        var userIds = users.Select(u => u.Id).ToList();

        // Get department names
        var departments = await _departmentRepository.GetQueryable().ToListAsync(cancellationToken);
        var departmentLookup = departments.ToDictionary(d => d.Id, d => d.Name);

        // Get all attendance logs for the month
        var attendanceLogs = await _attendanceRepository.GetQueryable()
            .Where(a => userIds.Contains(a.UserId) && a.Date >= startDate && a.Date <= endDate)
            .ToListAsync(cancellationToken);

        // Get all approved leave requests that overlap with this month
        var leaveRequests = await _leaveRequestRepository.GetQueryable()
            .Where(r => userIds.Contains(r.UserId) && 
                        r.Status == RequestStatus.Approved &&
                        r.StartDate <= endDate && r.EndDate >= startDate)
            .ToListAsync(cancellationToken);

        // Get all approved WFH requests that overlap with this month
        var wfhRequests = await _workFromHomeRequestRepository.GetQueryable()
            .Where(r => userIds.Contains(r.UserId) && 
                        r.Status == RequestStatus.Approved &&
                        r.FromDate <= endDate && r.ToDate >= startDate)
            .ToListAsync(cancellationToken);

        // Get all approved time-off requests for this month
        var timeOffRequests = await _timeOffRequestRepository.GetQueryable()
            .Where(r => userIds.Contains(r.UserId) && 
                        r.Status == RequestStatus.Approved &&
                        r.Date >= startDate && r.Date <= endDate)
            .ToListAsync(cancellationToken);

        var results = new List<MonthlyWorkSummaryDto>();

        foreach (var user in users)
        {
            var userAttendance = attendanceLogs.Where(a => a.UserId == user.Id).ToList();
            var userLeaves = leaveRequests.Where(r => r.UserId == user.Id).ToList();
            var userWfh = wfhRequests.Where(r => r.UserId == user.Id).ToList();
            var userTimeOff = timeOffRequests.Where(r => r.UserId == user.Id).ToList();

            decimal attendanceHours = 0;
            decimal leaveHours = 0;
            decimal wfhHours = 0;
            decimal timeOffHours = 0;
            int daysPresent = 0;
            int daysOnLeave = 0;
            int daysWfh = 0;
            int daysAbsent = 0;
            bool timeOffUsedThisMonth = false;

            foreach (var workDay in workingDays)
            {
                var attendance = userAttendance.FirstOrDefault(a => a.Date.Date == workDay.Date);
                
                if (attendance != null)
                {
                    // Employee has attendance record
                    attendanceHours += attendance.HoursWorked;
                    daysPresent++;

                    // Check if time-off applies (attendance < 8 hours and not yet used this month)
                    if (attendance.HoursWorked < StandardWorkHoursPerDay && !timeOffUsedThisMonth)
                    {
                        var timeOff = userTimeOff.FirstOrDefault(t => t.Date.Date == workDay.Date);
                        if (timeOff != null)
                        {
                            timeOffHours += TimeOffHoursPerRequest;
                            timeOffUsedThisMonth = true;
                        }
                    }
                }
                else
                {
                    // No attendance - check for leave
                    var leave = userLeaves.FirstOrDefault(l => workDay.Date >= l.StartDate.Date && workDay.Date <= l.EndDate.Date);
                    if (leave != null)
                    {
                        leaveHours += StandardWorkHoursPerDay;
                        daysOnLeave++;
                    }
                    else
                    {
                        // Check for WFH
                        var wfh = userWfh.FirstOrDefault(w => workDay.Date >= w.FromDate.Date && workDay.Date <= w.ToDate.Date);
                        if (wfh != null)
                        {
                            wfhHours += StandardWorkHoursPerDay;
                            daysWfh++;
                        }
                        else
                        {
                            // Absent
                            daysAbsent++;
                        }
                    }
                }
            }

            var departmentName = user.DepartmentId.HasValue && departmentLookup.ContainsKey(user.DepartmentId.Value)
                ? departmentLookup[user.DepartmentId.Value]
                : "N/A";

            results.Add(new MonthlyWorkSummaryDto
            {
                UserId = user.Id,
                EmployeeName = user.FullName,
                DepartmentName = departmentName,
                Year = year,
                Month = month,
                WorkingDaysInMonth = workingDaysCount,
                ExpectedHours = expectedHours,
                ActualAttendanceHours = attendanceHours,
                LeaveHours = leaveHours,
                WorkFromHomeHours = wfhHours,
                TimeOffHours = timeOffHours,
                TotalHoursWorked = attendanceHours + leaveHours + wfhHours + timeOffHours,
                DaysPresent = daysPresent,
                DaysOnLeave = daysOnLeave,
                DaysWorkFromHome = daysWfh,
                DaysAbsent = daysAbsent
            });
        }

        return results.OrderBy(r => r.DepartmentName).ThenBy(r => r.EmployeeName);
    }

    public async Task<MonthlyWorkDetailsDto?> GetMonthlyWorkDetailsReportAsync(
        Guid employeeId,
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetQueryable()
            .FirstOrDefaultAsync(u => u.Id == employeeId, cancellationToken);
        
        if (user == null) return null;

        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var workingDays = GetWorkingDaysInMonth(year, month);

        // Get department name
        string departmentName = "N/A";
        if (user.DepartmentId.HasValue)
        {
            var department = await _departmentRepository.GetQueryable()
                .FirstOrDefaultAsync(d => d.Id == user.DepartmentId.Value, cancellationToken);
            if (department != null) departmentName = department.Name;
        }

        // Get attendance logs
        var attendanceLogs = await _attendanceRepository.GetQueryable()
            .Where(a => a.UserId == employeeId && a.Date >= startDate && a.Date <= endDate)
            .ToListAsync(cancellationToken);

        // Get approved leave requests
        var leaveRequests = await _leaveRequestRepository.GetQueryable()
            .Where(r => r.UserId == employeeId && 
                        r.Status == RequestStatus.Approved &&
                        r.StartDate <= endDate && r.EndDate >= startDate)
            .ToListAsync(cancellationToken);

        // Get approved WFH requests
        var wfhRequests = await _workFromHomeRequestRepository.GetQueryable()
            .Where(r => r.UserId == employeeId && 
                        r.Status == RequestStatus.Approved &&
                        r.FromDate <= endDate && r.ToDate >= startDate)
            .ToListAsync(cancellationToken);

        // Get approved time-off requests
        var timeOffRequests = await _timeOffRequestRepository.GetQueryable()
            .Where(r => r.UserId == employeeId && 
                        r.Status == RequestStatus.Approved &&
                        r.Date >= startDate && r.Date <= endDate)
            .ToListAsync(cancellationToken);

        var dailyDetails = new List<DailyWorkDetailDto>();
        bool timeOffUsedThisMonth = false;
        
        decimal totalAttendanceHours = 0;
        decimal totalLeaveHours = 0;
        decimal totalWfhHours = 0;
        decimal totalTimeOffHours = 0;
        int totalDaysPresent = 0;
        int totalDaysOnLeave = 0;
        int totalDaysWfh = 0;
        int totalDaysAbsent = 0;

        for (int day = 1; day <= daysInMonth; day++)
        {
            var date = new DateTime(year, month, day);
            var isWeekend = IsWeekend(date);
            
            var detail = new DailyWorkDetailDto
            {
                Date = date,
                DayOfWeek = date.DayOfWeek.ToString(),
                IsWeekend = isWeekend
            };

            if (isWeekend)
            {
                detail.Status = "Weekend";
                detail.TotalHours = 0;
            }
            else
            {
                var attendance = attendanceLogs.FirstOrDefault(a => a.Date.Date == date.Date);
                
                if (attendance != null)
                {
                    detail.Status = "Present";
                    detail.AttendanceHours = attendance.HoursWorked;
                    detail.CheckInTime = attendance.CheckInTime;
                    detail.CheckOutTime = attendance.CheckOutTime;
                    detail.Notes = attendance.Notes;
                    totalAttendanceHours += attendance.HoursWorked;
                    totalDaysPresent++;

                    // Check for time-off
                    if (attendance.HoursWorked < StandardWorkHoursPerDay && !timeOffUsedThisMonth)
                    {
                        var timeOff = timeOffRequests.FirstOrDefault(t => t.Date.Date == date.Date);
                        if (timeOff != null)
                        {
                            detail.TimeOffHours = TimeOffHoursPerRequest;
                            totalTimeOffHours += TimeOffHoursPerRequest;
                            timeOffUsedThisMonth = true;
                        }
                    }

                    detail.TotalHours = detail.AttendanceHours + detail.TimeOffHours;
                }
                else
                {
                    // Check for leave
                    var leave = leaveRequests.FirstOrDefault(l => date.Date >= l.StartDate.Date && date.Date <= l.EndDate.Date);
                    if (leave != null)
                    {
                        detail.Status = "Leave";
                        detail.LeaveType = leave.LeaveType.ToString();
                        detail.TotalHours = StandardWorkHoursPerDay;
                        totalLeaveHours += StandardWorkHoursPerDay;
                        totalDaysOnLeave++;
                    }
                    else
                    {
                        // Check for WFH
                        var wfh = wfhRequests.FirstOrDefault(w => date.Date >= w.FromDate.Date && date.Date <= w.ToDate.Date);
                        if (wfh != null)
                        {
                            detail.Status = "WFH";
                            detail.TotalHours = StandardWorkHoursPerDay;
                            totalWfhHours += StandardWorkHoursPerDay;
                            totalDaysWfh++;
                        }
                        else
                        {
                            detail.Status = "Absent";
                            detail.TotalHours = 0;
                            totalDaysAbsent++;
                        }
                    }
                }
            }

            dailyDetails.Add(detail);
        }

        return new MonthlyWorkDetailsDto
        {
            UserId = user.Id,
            EmployeeName = user.FullName,
            DepartmentName = departmentName,
            Year = year,
            Month = month,
            DailyDetails = dailyDetails,
            WorkingDaysInMonth = workingDays.Count,
            ExpectedHours = workingDays.Count * StandardWorkHoursPerDay,
            TotalAttendanceHours = totalAttendanceHours,
            TotalLeaveHours = totalLeaveHours,
            TotalWorkFromHomeHours = totalWfhHours,
            TotalTimeOffHours = totalTimeOffHours,
            GrandTotalHours = totalAttendanceHours + totalLeaveHours + totalWfhHours + totalTimeOffHours,
            TotalDaysPresent = totalDaysPresent,
            TotalDaysOnLeave = totalDaysOnLeave,
            TotalDaysWorkFromHome = totalDaysWfh,
            TotalDaysAbsent = totalDaysAbsent
        };
    }
}
