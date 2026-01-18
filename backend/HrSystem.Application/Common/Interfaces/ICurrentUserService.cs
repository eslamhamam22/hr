namespace HrSystem.Application.Common.Interfaces;

/// <summary>
/// Interface to retrieve current user information from HTTP context
/// </summary>
public interface ICurrentUserService
{
    Guid UserId { get; }
    string Username { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
}
