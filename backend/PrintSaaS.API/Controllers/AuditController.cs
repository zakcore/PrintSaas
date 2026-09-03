using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrintSaaS.Data.Repositories;

namespace PrintSaaS.API.Controllers;

[ApiController]
[Route("api/admin/audit")]
[Authorize(Roles = "Admin")]
public class AuditController : ControllerBase
{
    private readonly IAuditRepository _auditRepo;

    public AuditController(IAuditRepository auditRepo)
    {
        _auditRepo = auditRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? jobId, [FromQuery] int skip = 0, [FromQuery] int take = 100)
    {
        var entries = await _auditRepo.GetAllAsync(jobId, skip, take);
        return Ok(entries);
    }
}
