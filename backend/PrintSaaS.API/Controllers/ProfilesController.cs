using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintSaaS.Core.Services;
using PrintSaaS.Models;

namespace PrintSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfilesController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly ILogger<ProfilesController> _logger;

    public ProfilesController(IProfileService profileService, ILogger<ProfilesController> logger)
    {
        _profileService = profileService;
        _logger = logger;
    }

    // --- Queue Profiles ---

    [HttpGet("queue")]
    public async Task<IActionResult> GetQueueProfiles([FromQuery] int? printerId)
    {
        var profiles = await _profileService.GetQueueProfilesAsync(printerId);
        return Ok(profiles);
    }

    [HttpGet("queue/{id}")]
    public async Task<IActionResult> GetQueueProfile(int id)
    {
        var profile = await _profileService.GetQueueProfileByIdAsync(id);
        if (profile is null) return NotFound();
        return Ok(profile);
    }

    [HttpPost("queue")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateQueueProfile([FromBody] QueueProfile profile)
    {
        var created = await _profileService.CreateQueueProfileAsync(profile);
        _logger.LogInformation("Queue profile {DisplayName} created for printer {PrinterId}",
            created.DisplayName, created.PrinterId);
        return CreatedAtAction(nameof(GetQueueProfile), new { id = created.Id }, created);
    }

    [HttpPut("queue/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateQueueProfile(int id, [FromBody] QueueProfile profile)
    {
        var result = await _profileService.UpdateQueueProfileAsync(id, profile);
        if (!result) return NotFound();
        _logger.LogInformation("Queue profile {ProfileId} updated", id);
        return Ok(new { message = "Queue profile updated" });
    }

    [HttpDelete("queue/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateQueueProfile(int id)
    {
        var result = await _profileService.DeactivateQueueProfileAsync(id);
        if (!result) return NotFound();
        _logger.LogInformation("Queue profile {ProfileId} deactivated", id);
        return Ok(new { message = "Queue profile deactivated" });
    }

    public record SuggestRequest(int PrinterId, bool IsDuplex, string ColorMode);

    [HttpPost("suggest")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> SuggestProfile([FromBody] SuggestRequest request)
    {
        var profile = await _profileService.SuggestProfileAsync(
            request.PrinterId, request.IsDuplex, request.ColorMode);

        if (profile is null)
            return NotFound(new { message = "No matching profile found" });

        return Ok(profile);
    }

    // --- Tray Profiles ---

    [HttpGet("tray")]
    public async Task<IActionResult> GetTrayProfiles([FromQuery] int? printerId)
    {
        var profiles = await _profileService.GetTrayProfilesAsync(printerId);
        return Ok(profiles);
    }

    [HttpGet("tray/{id}")]
    public async Task<IActionResult> GetTrayProfile(int id)
    {
        var profile = await _profileService.GetTrayProfileByIdAsync(id);
        if (profile is null) return NotFound();
        return Ok(profile);
    }

    [HttpPost("tray")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateTrayProfile([FromBody] TrayProfile profile)
    {
        var created = await _profileService.CreateTrayProfileAsync(profile);
        _logger.LogInformation("Tray profile created for printer {PrinterId}, tray {TrayNumber}",
            created.PrinterId, created.TrayNumber);
        return CreatedAtAction(nameof(GetTrayProfile), new { id = created.Id }, created);
    }

    [HttpPut("tray/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateTrayProfile(int id, [FromBody] TrayProfile profile)
    {
        var result = await _profileService.UpdateTrayProfileAsync(id, profile);
        if (!result) return NotFound();
        _logger.LogInformation("Tray profile {ProfileId} updated", id);
        return Ok(new { message = "Tray profile updated" });
    }

    [HttpDelete("tray/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeactivateTrayProfile(int id)
    {
        var result = await _profileService.DeactivateTrayProfileAsync(id);
        if (!result) return NotFound();
        _logger.LogInformation("Tray profile {ProfileId} deactivated", id);
        return Ok(new { message = "Tray profile deactivated" });
    }
}
