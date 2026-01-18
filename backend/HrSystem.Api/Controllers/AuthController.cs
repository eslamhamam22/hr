using HrSystem.Application.Common.Interfaces;
using HrSystem.Application.DTOs.Auth;
using HrSystem.Application.Services.Auth;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.Api.Controllers;

/// <summary>
/// Authentication controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Login endpoint
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var (succeeded, token, message, user) = await _authService.LoginAsync(
            request.Username,
            request.Password,
            cancellationToken);

        if (!succeeded)
        {
            return Unauthorized(new LoginResponse { Success = false, Message = message });
        }

        return Ok(new LoginResponse
        {
            Success = true,
            Token = token,
            Message = "Login successful",
            User = user
        });
    }
}
