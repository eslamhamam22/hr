using HrSystem.Domain.Entities;
using HrSystem.Domain.Interfaces;
using HrSystem.Infrastructure.Data.Context;

namespace HrSystem.Infrastructure.Data.Repositories;

/// <summary>
/// Generic EF Core repository implementation
/// </summary>
public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly HrDbContext _context;

    public EfRepository(HrDbContext context)
    {
        _context = context;
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().FindAsync(new object[] { id }, cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(_context.Set<T>().AsEnumerable());
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Custom repository for leave requests with specialized queries
/// </summary>
public class LeaveRepository : EfRepository<LeaveRequest>
{
    public LeaveRepository(HrDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<LeaveRequest>> GetPendingRequestsForManagerAsync(Guid managerId)
    {
        return await Task.FromResult(
            // Custom query logic would go here
            new List<LeaveRequest>());
    }
}
