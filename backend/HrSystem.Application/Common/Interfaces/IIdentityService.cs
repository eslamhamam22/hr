namespace HrSystem.Application.Common.Interfaces;

/// <summary>
/// Identity and authentication service interface
/// </summary>
public interface IIdentityService
{
    Task<(bool Succeeded, string? Token, string? Message)> LoginAsync(
        string username, 
        string password, 
        CancellationToken cancellationToken = default);
    
    Task<bool> VerifyPasswordAsync(string passwordHash, string password);
    
    string HashPassword(string password);
}
