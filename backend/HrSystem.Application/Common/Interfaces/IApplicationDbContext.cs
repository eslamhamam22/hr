namespace HrSystem.Application.Common.Interfaces;

/// <summary>
/// Application database context interface
/// </summary>
public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
