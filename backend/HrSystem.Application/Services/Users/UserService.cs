using HrSystem.Application.Common.Interfaces;
using HrSystem.Application.Common.Models;
using HrSystem.Application.DTOs.User;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Services.Users;

public class UserService : IUserService
{
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<Department> _departmentRepository;

    public UserService(IRepository<User> userRepository, IRepository<Department> departmentRepository)
    {
        _userRepository = userRepository;
        _departmentRepository = departmentRepository;
    }

    public async Task<PaginatedResult<UserDto>> GetUsersAsync(
        int page,
        int pageSize,
        string? search,
        Guid? departmentId,
        CancellationToken cancellationToken = default)
    {
        var usersQuery = _userRepository.GetQueryable();
        var departmentsQuery = _departmentRepository.GetQueryable();

        // Join manually since navigation properties might be missing or not configured
        var query = from u in usersQuery
                    join d in departmentsQuery on u.DepartmentId equals d.Id into deptGroup
                    from dept in deptGroup.DefaultIfEmpty()
                    join m in usersQuery on u.ManagerId equals m.Id into managerGroup
                    from mgr in managerGroup.DefaultIfEmpty()
                    select new { User = u, DepartmentName = dept.Name, ManagerName = mgr.FullName };

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.User.Username.Contains(search) || 
                                     x.User.FullName.Contains(search) || 
                                     x.User.Email.Contains(search));
        }

        if (departmentId.HasValue)
        {
            query = query.Where(x => x.User.DepartmentId == departmentId);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.User.Username)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserDto
            {
                Id = x.User.Id,
                Username = x.User.Username,
                FullName = x.User.FullName,
                Email = x.User.Email,
                Role = x.User.Role,
                ManagerId = x.User.ManagerId,
                ManagerName = x.ManagerName,
                DepartmentId = x.User.DepartmentId,
                DepartmentName = x.DepartmentName,
                IsActive = x.User.IsActive,
                CreatedAt = x.User.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResult<UserDto>(items, totalCount, page, pageSize);
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var usersQuery = _userRepository.GetQueryable();
        var departmentsQuery = _departmentRepository.GetQueryable();

        var user = await (from u in usersQuery
                          where u.Id == id
                          join d in departmentsQuery on u.DepartmentId equals d.Id into deptGroup
                          from dept in deptGroup.DefaultIfEmpty()
                          join m in usersQuery on u.ManagerId equals m.Id into managerGroup
                          from mgr in managerGroup.DefaultIfEmpty()
                          select new UserDto
                          {
                              Id = u.Id,
                              Username = u.Username,
                              FullName = u.FullName,
                              Email = u.Email,
                              Role = u.Role,
                              ManagerId = u.ManagerId,
                              ManagerName = mgr.FullName,
                              DepartmentId = u.DepartmentId,
                              DepartmentName = dept.Name,
                              IsActive = u.IsActive,
                              CreatedAt = u.CreatedAt
                          }).FirstOrDefaultAsync(cancellationToken);

        return user;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        if (await _userRepository.GetQueryable().AnyAsync(u => u.Username == dto.Username, cancellationToken))
        {
            throw new Exception("Username already exists");
        }
        
        if (await _userRepository.GetQueryable().AnyAsync(u => u.Email == dto.Email, cancellationToken))
        {
             throw new Exception("Email already exists");
        }

        var user = new User
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FullName = dto.FullName,
            Email = dto.Email,
            Role = dto.Role,
            ManagerId = dto.ManagerId,
            DepartmentId = dto.DepartmentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return await GetUserByIdAsync(user.Id, cancellationToken) ?? new UserDto();
    }

    public async Task<bool> UpdateUserAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null) return false;

        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.Role = dto.Role;
        user.ManagerId = dto.ManagerId;
        user.DepartmentId = dto.DepartmentId;
        user.IsActive = dto.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(dto.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null) return false;

        _userRepository.Delete(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
