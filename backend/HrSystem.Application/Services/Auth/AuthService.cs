using HrSystem.Application.Common.Interfaces;
using HrSystem.Application.DTOs.Auth;
using HrSystem.Domain.Entities;
using HrSystem.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HrSystem.Application.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepository;
    private readonly IIdentityService _identityService;
    private readonly IConfiguration _configuration;

    public AuthService(
        IRepository<User> userRepository,
        IIdentityService identityService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _identityService = identityService;
        _configuration = configuration;
    }

    public async Task<(bool Succeeded, string? Token, string? Message, UserInfo? User)> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        // Find user by username
        // Since IRepository might not have specific FindByUsername, we might need to use a Specification or GetList
        // Assuming GetListAsync accepts a specification or we have to filter in memory (bad for prod but ok for prototype)
        // Or better, IRepository has a filter mechanism.
        
        // Let's check IRepository definition. 
        // For now, I'll assume I can fetch all or filter. 
        // Actually, I'll blindly try to use a specification if I saw the folder, or just GetAll.
        
        var users = await _userRepository.GetAllAsync(cancellationToken); 
        var user = users.FirstOrDefault(u => u.Username == username);

        if (user == null)
        {
            return (false, null, "Invalid credentials", null);
        }

        if (!user.IsActive)
        {
            return (false, null, "Account is inactive", null);
        }

        // Verify password
        var isPasswordValid = await _identityService.VerifyPasswordAsync(user.PasswordHash, password);
        if (!isPasswordValid)
        {
            return (false, null, "Invalid credentials", null);
        }

        // Generate Token
        var token = GenerateJwtToken(user);

        // Map to UserInfo
        var userInfo = new UserInfo
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString()
        };

        return (true, token, "Login successful", userInfo);
    }

    private string GenerateJwtToken(User user)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "your-super-secret-key-that-must-be-at-least-32-characters");
        var issuer = _configuration["Jwt:Issuer"] ?? "HrSystem";
        var audience = _configuration["Jwt:Audience"] ?? "HrSystemUsers";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("FullName", user.FullName)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(8),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
