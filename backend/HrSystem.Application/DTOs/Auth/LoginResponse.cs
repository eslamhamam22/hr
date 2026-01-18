namespace HrSystem.Application.DTOs.Auth;

/// <summary>
/// Login response DTO
/// </summary>
public class LoginResponse
{
    public bool Success { get; set; }
    
    public string? Token { get; set; }
    
    public string? Message { get; set; }
    
    public UserInfo? User { get; set; }
}

public class UserInfo
{
    public Guid Id { get; set; }
    
    public string Username { get; set; } = string.Empty;
    
    public string FullName { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string Role { get; set; } = string.Empty;
}
