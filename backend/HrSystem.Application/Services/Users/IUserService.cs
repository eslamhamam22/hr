using HrSystem.Application.Common.Models;
using HrSystem.Application.DTOs.User;

namespace HrSystem.Application.Services.Users;

public interface IUserService
{
    Task<PaginatedResult<UserDto>> GetUsersAsync(int page, int pageSize, string? search, Guid? departmentId, CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserDto> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
}
