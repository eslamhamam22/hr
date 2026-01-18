using HrSystem.Domain.Entities;
using HrSystem.Domain.Interfaces;
using HrSystem.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

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

    public IQueryable<T> GetQueryable()
    {
        return _context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().FindAsync(new object[] { id }, cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().ToListAsync(cancellationToken);
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddAsync(entity, cancellationToken);
    }

    public void Update(T entity)
    {
        _context.Set<T>().Update(entity);
    }

    public void Delete(T entity)
    {
        _context.Set<T>().Remove(entity);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
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
