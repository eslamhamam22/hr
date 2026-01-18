using HrSystem.Application.DTOs.Auth;

namespace HrSystem.Application.Services.Auth;

/// <summary>
/// Application service for authentication use cases
/// </summary>
public interface IAuthService
{
    Task<(bool Succeeded, string? Token, string? Message, UserInfo? User)> LoginAsync(
        string username, 
        string password, 
        CancellationToken cancellationToken = default);
}
