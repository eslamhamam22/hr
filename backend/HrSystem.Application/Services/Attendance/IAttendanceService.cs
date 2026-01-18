using HrSystem.Application.Common.Models;
using HrSystem.Application.DTOs.Attendance;

namespace HrSystem.Application.Services.Attendance;

public interface IAttendanceService
{
    Task<PaginatedResult<AttendanceLogDto>> GetAttendanceLogsAsync(
        int page,
        int pageSize,
        Guid? userId,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default);

    Task<AttendanceLogDto?> GetAttendanceLogByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
