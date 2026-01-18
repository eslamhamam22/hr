using HrSystem.Application.Common.Interfaces;
using HrSystem.Application.Common.Models;
using HrSystem.Application.DTOs.Attendance;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Services.Attendance;

public class AttendanceService : IAttendanceService
{
    private readonly IRepository<AttendanceLog> _attendanceRepository;

    public AttendanceService(IRepository<AttendanceLog> attendanceRepository)
    {
        _attendanceRepository = attendanceRepository;
    }

    public async Task<PaginatedResult<AttendanceLogDto>> GetAttendanceLogsAsync(
        int page,
        int pageSize,
        Guid? userId,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        var queryE = await _attendanceRepository.GetAllAsync(cancellationToken);
        var query = queryE.AsQueryable();

        // Apply filters
        if (userId.HasValue)
        {
            query = query.Where(a => a.UserId == userId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(a => a.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.Date <= endDate.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .OrderByDescending(a => a.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AttendanceLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                //UserName = a.User.FullName,
                Date = a.Date,
                CheckInTime = a.CheckInTime.ToString(),
                CheckOutTime = a.CheckOutTime.ToString(),
                HoursWorked = a.HoursWorked,
                IsLate = a.IsLate,
                IsAbsent = a.IsAbsent,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResult<AttendanceLogDto>(items, totalCount, page, pageSize);
    }

    public async Task<AttendanceLogDto?> GetAttendanceLogByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var queryE = await _attendanceRepository.GetAllAsync(cancellationToken);
        var query = queryE.AsQueryable();
        var attendance = await query
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (attendance == null)
            return null;

        return new AttendanceLogDto
        {
            Id = attendance.Id,
            UserId = attendance.UserId,
            //UserName = attendance.User?.FullName,
            Date = attendance.Date,
            CheckInTime = attendance.CheckInTime.ToString(),
            CheckOutTime = attendance.CheckOutTime.ToString(),
            HoursWorked = attendance.HoursWorked,
            IsLate = attendance.IsLate,
            IsAbsent = attendance.IsAbsent,
            Notes = attendance.Notes,
            CreatedAt = attendance.CreatedAt
        };
    }
}
