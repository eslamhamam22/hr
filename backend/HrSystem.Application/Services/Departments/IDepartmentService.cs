using HrSystem.Application.Common.Models;
using HrSystem.Application.DTOs.Department;

namespace HrSystem.Application.Services.Departments;

public interface IDepartmentService
{
    Task<PaginatedResult<DepartmentDto>> GetDepartmentsAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default);

    Task<DepartmentDto?> GetDepartmentByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentDto dto, CancellationToken cancellationToken = default);

    Task<bool> UpdateDepartmentAsync(Guid id, UpdateDepartmentDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteDepartmentAsync(Guid id, CancellationToken cancellationToken = default);
}
