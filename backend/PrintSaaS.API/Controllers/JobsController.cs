using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PrintSaaS.API.Hubs;
using PrintSaaS.Core.Services;
using PrintSaaS.Engine;
using PrintSaaS.Models;
using PrintSaaS.Models.Enums;

namespace PrintSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;
    private readonly IFileEncryptionService _encryption;
    private readonly IHubContext<PrintStatusHub> _hub;
    private readonly ILogger<JobsController> _logger;
    private readonly IConfiguration _config;

    public JobsController(
        IJobService jobService,
        IFileEncryptionService encryption,
        IHubContext<PrintStatusHub> hub,
        ILogger<JobsController> logger,
        IConfiguration config)
    {
        _jobService = jobService;
        _encryption = encryption;
        _hub = hub;
        _logger = logger;
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] JobStatus? status, [FromQuery] string? clientName)
    {
        var jobs = await _jobService.GetAllAsync(status, clientName);
        return Ok(jobs);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var job = await _jobService.GetByIdAsync(id);
        if (job is null) return NotFound();
        return Ok(job);
    }

    [HttpPost("{id}/send")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> Send(int id)
    {
        var operatorId = GetOperatorId();
        var result = await _jobService.SendAsync(id, operatorId);

        if (!result) return BadRequest(new { message = "Job cannot be sent in its current state" });

        _logger.LogInformation("Job {JobId} sent by operator {OperatorId}", id, operatorId);

        await _hub.Clients.All.SendAsync("OnJobStatusChanged", new
        {
            jobId = id,
            status = JobStatus.Pending.ToString(),
            timestamp = DateTime.UtcNow
        });

        return Ok(new { message = "Job sent to print queue" });
    }

    [HttpPost("{id}/resend")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> Resend(int id)
    {
        var operatorId = GetOperatorId();
        var result = await _jobService.ResendAsync(id, operatorId);

        if (!result) return BadRequest(new { message = "Job cannot be resent — must be in Retained status" });

        _logger.LogInformation("Job {JobId} resent by operator {OperatorId}", id, operatorId);

        await _hub.Clients.All.SendAsync("OnJobStatusChanged", new
        {
            jobId = id,
            status = JobStatus.Pending.ToString(),
            timestamp = DateTime.UtcNow
        });

        return Ok(new { message = "Job resent from retain queue" });
    }

    public record ConfirmRequest(string? ComplianceNote);

    [HttpPost("{id}/confirm")]
    [Authorize(Roles = "Admin,Operator")]
    public async Task<IActionResult> Confirm(int id, [FromBody] ConfirmRequest request)
    {
        var operatorId = GetOperatorId();
        var result = await _jobService.ConfirmAsync(id, operatorId, request.ComplianceNote);

        if (!result) return BadRequest(new { message = "Job is not awaiting confirmation" });

        _logger.LogInformation("Job {JobId} confirmed by operator {OperatorId}", id, operatorId);

        await _hub.Clients.All.SendAsync("OnJobStatusChanged", new
        {
            jobId = id,
            status = JobStatus.Pending.ToString(),
            timestamp = DateTime.UtcNow
        });

        return Ok(new { message = "Job confirmed and released" });
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Admin,Operator")]
    [RequestSizeLimit(500_000_000)] // 500 MB max
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        [FromForm] string jobName,
        [FromForm] string clientName,
        [FromForm] string jobType,
        [FromForm] bool isDuplex,
        [FromForm] string colorMode,
        [FromForm] int copies,
        [FromForm] bool isPayrollJob,
        [FromForm] int? employeeCount,
        [FromForm] int? pagesPerEmployee,
        [FromForm] bool containsCheques,
        [FromForm] bool containsStubs,
        [FromForm] string paperType,
        [FromForm] int? printerId,
        [FromForm] int? queueProfileId)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file uploaded" });

        if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Only PDF files are accepted" });

        var operatorId = GetOperatorId();

        // Encrypt and store — never write plaintext to disk
        var storageDir = _config["Storage:EncryptedFilesPath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "EncryptedFiles");
        Directory.CreateDirectory(storageDir);

        var encryptedFileName = $"{Guid.NewGuid()}.enc";
        var encryptedFilePath = Path.Combine(storageDir, encryptedFileName);

        await using (var stream = file.OpenReadStream())
        {
            await _encryption.EncryptAndSaveAsync(stream, encryptedFilePath);
        }

        // Count pages (approximate from file size if needed — real count from PDF library later)
        var totalPageCount = 0;
        if (isPayrollJob && employeeCount.HasValue && pagesPerEmployee.HasValue)
            totalPageCount = employeeCount.Value * pagesPerEmployee.Value;

        // Parse job type
        if (!Enum.TryParse<JobType>(jobType, true, out var parsedJobType))
            parsedJobType = JobType.BankStatement;

        var job = new Job
        {
            JobName = jobName,
            ClientName = clientName,
            FileLocation = encryptedFilePath,
            FileSize = file.Length,
            JobType = parsedJobType,
            PrinterId = printerId,
            QueueProfileId = queueProfileId,
            OperatorId = operatorId,
            Parameters = new JobParameters
            {
                IsDuplex = isDuplex,
                ColorMode = colorMode,
                Copies = copies,
                TotalPageCount = totalPageCount,
                IsPayrollJob = isPayrollJob,
                EmployeeCount = employeeCount,
                PagesPerEmployee = pagesPerEmployee,
                ContainsCheques = containsCheques,
                ContainsStubs = containsStubs,
                PaperType = paperType
            }
        };

        var created = await _jobService.CreateAsync(job);

        _logger.LogInformation(
            "Job {JobName} uploaded by operator {OperatorId}, file size {FileSize} bytes, encrypted at {Path}",
            jobName, operatorId, file.Length, encryptedFilePath);

        await _hub.Clients.All.SendAsync("OnJobStatusChanged", new
        {
            jobId = created.Id,
            status = JobStatus.Received.ToString(),
            timestamp = DateTime.UtcNow
        });

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id}/history")]
    public async Task<IActionResult> GetHistory(int id)
    {
        var history = await _jobService.GetHistoryAsync(id);
        return Ok(history);
    }

    private int GetOperatorId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null ? int.Parse(claim.Value) : 0;
    }
}
