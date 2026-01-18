using HrSystem.Application.Common.Interfaces;

namespace HrSystem.Api.Services;

/// <summary>
/// Implementation of current user service from HTTP context
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var userId = _httpContextAccessor.HttpContext?.Items["UserId"]?.ToString();
            return Guid.TryParse(userId, out var guid) ? guid : Guid.Empty;
        }
    }

    public string Username
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? string.Empty;
        }
    }

    public string? Role
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.Claims
                .FirstOrDefault(x => x.Type == "role")?.Value;
        }
    }

    public bool IsAuthenticated
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        }
    }
}
