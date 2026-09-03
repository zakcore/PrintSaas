using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintSaaS.Core.Services;
using PrintSaaS.Models.Enums;

namespace PrintSaaS.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var users = await _userService.GetAllAsync(includeInactive);
        return Ok(users.Select(u => new
        {
            u.Id, u.Username, u.DisplayName,
            Role = u.Role.ToString(), u.IsActive, u.CreatedAt
        }));
    }

    public record CreateUserRequest(string Username, string Password, string DisplayName, UserRole Role);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var user = await _userService.CreateAsync(
            request.Username, request.Password, request.DisplayName, request.Role);

        _logger.LogInformation("User {Username} created with role {Role}", user.Username, user.Role);

        return CreatedAtAction(nameof(GetAll), new
        {
            user.Id, user.Username, user.DisplayName,
            Role = user.Role.ToString(), user.IsActive
        });
    }

    public record UpdateUserRequest(string? DisplayName, UserRole? Role);

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequest request)
    {
        var result = await _userService.UpdateAsync(id, request.DisplayName, request.Role);
        if (!result) return NotFound();
        _logger.LogInformation("User {UserId} updated", id);
        return Ok(new { message = "User updated" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await _userService.DeactivateAsync(id);
        if (!result) return NotFound();
        _logger.LogInformation("User {UserId} deactivated", id);
        return Ok(new { message = "User deactivated" });
    }
}
