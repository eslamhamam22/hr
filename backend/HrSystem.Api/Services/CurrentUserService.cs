using HrSystem.Application.Common.Interfaces;
using System.Security.Claims;

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
            // First try to get from Items (set by JwtMiddleware)
            var userId = _httpContextAccessor.HttpContext?.Items["UserId"]?.ToString();
            if (Guid.TryParse(userId, out var guid))
            {
                return guid;
            }

            // Fallback: Try to get from authenticated user claims
            var claims = _httpContextAccessor.HttpContext?.User?.Claims;
            if (claims != null)
            {
                var userIdClaim = claims.FirstOrDefault(c => 
                    c.Type == ClaimTypes.NameIdentifier || 
                    c.Type == "sub" || 
                    c.Type == "nameid")?.Value;
                    
                if (Guid.TryParse(userIdClaim, out var claimGuid))
                {
                    return claimGuid;
                }
            }

            return Guid.Empty;
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
                .FirstOrDefault(x => x.Type == ClaimTypes.Role || x.Type == "role")?.Value;
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
