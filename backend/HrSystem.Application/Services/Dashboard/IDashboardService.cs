using HrSystem.Application.DTOs.Dashboard;

namespace HrSystem.Application.Services.Dashboard;

public interface IDashboardService
{
    Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ManagerDashboardDto> GetManagerDashboardAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AdminDashboardDto> GetAdminDashboardAsync(Guid userId, CancellationToken cancellationToken = default);
}
