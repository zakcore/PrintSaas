using Microsoft.AspNetCore.Mvc;
using PrintSaaS.Core.Services;

namespace PrintSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public record LoginRequest(string Username, string Password);
    public record LoginResponse(string Token, string Username, string DisplayName, string Role);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var (user, token) = await _authService.LoginAsync(request.Username, request.Password);

        if (user is null || token is null)
        {
            _logger.LogWarning("Failed login attempt for user {Username}", request.Username);
            return Unauthorized(new { message = "Invalid credentials" });
        }

        _logger.LogInformation("User {Username} logged in", user.Username);

        return Ok(new LoginResponse(token, user.Username, user.DisplayName, user.Role.ToString()));
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        // JWT is stateless — client discards the token
        return Ok(new { message = "Logged out" });
    }
}
