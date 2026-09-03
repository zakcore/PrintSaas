using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PrintSaaS.API.Hubs;
using PrintSaaS.Core.Services;
using PrintSaaS.Engine;
using PrintSaaS.Models;

namespace PrintSaaS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrintersController : ControllerBase
{
    private readonly IPrinterService _printerService;
    private readonly IProfileService _profileService;
    private readonly IIppPrintSender _ippSender;
    private readonly IPrinterProfileDiscovery _discovery;
    private readonly IPrinterMonitor _monitor;
    private readonly IHubContext<PrintStatusHub> _hub;
    private readonly ILogger<PrintersController> _logger;

    public PrintersController(
        IPrinterService printerService,
        IProfileService profileService,
        IIppPrintSender ippSender,
        IPrinterProfileDiscovery discovery,
        IPrinterMonitor monitor,
        IHubContext<PrintStatusHub> hub,
        ILogger<PrintersController> logger)
    {
        _printerService = printerService;
        _profileService = profileService;
        _ippSender = ippSender;
        _discovery = discovery;
        _monitor = monitor;
        _hub = hub;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var printers = await _printerService.GetAllAsync(includeInactive);
        return Ok(printers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var printer = await _printerService.GetByIdAsync(id);
        if (printer is null) return NotFound();
        return Ok(printer);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Printer printer)
    {
        var created = await _printerService.CreateAsync(printer);
        _logger.LogInformation("Printer {PrinterName} created with IP {IpAddress}", created.Name, created.IpAddress);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] Printer printer)
    {
        var result = await _printerService.UpdateAsync(id, printer);
        if (!result) return NotFound();
        _logger.LogInformation("Printer {PrinterId} updated", id);
        return Ok(new { message = "Printer updated" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await _printerService.DeactivateAsync(id);
        if (!result) return NotFound();
        _logger.LogInformation("Printer {PrinterId} deactivated", id);
        return Ok(new { message = "Printer deactivated" });
    }

    [HttpGet("{id}/status")]
    public async Task<IActionResult> GetStatus(int id)
    {
        var printer = await _printerService.GetByIdAsync(id);
        if (printer is null) return NotFound();

        var status = await _monitor.GetPrinterStatusAsync(printer);
        return Ok(new
        {
            status.IsOnline,
            status.Status,
            printer.LastSeenOnline,
            status.TrayPaperLevels
        });
    }

    [HttpPost("{id}/test")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> TestConnection(int id)
    {
        var printer = await _printerService.GetByIdAsync(id);
        if (printer is null) return NotFound();

        var (success, error) = await _ippSender.TestConnectionAsync(printer);
        _logger.LogInformation("Test connection for {PrinterName}: {Success}", printer.Name, success);

        if (success)
            return Ok(new { message = $"Connection to {printer.Name} successful" });

        return BadRequest(new { message = $"Connection failed: {error}" });
    }

    [HttpPost("{id}/sync-queues")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SyncQueues(int id)
    {
        var printer = await _printerService.GetByIdAsync(id);
        if (printer is null) return NotFound();

        var queueNames = await _discovery.SyncQueueNamesAsync(id);
        _logger.LogInformation("Synced {Count} queues for {PrinterName}", queueNames.Count, printer.Name);

        await _hub.Clients.All.SendAsync("OnPrinterStatusChanged", new
        {
            printerId = id,
            isOnline = printer.IsOnline,
            message = $"Queue names synced: {queueNames.Count} found"
        });

        return Ok(new { message = $"Discovered {queueNames.Count} queue names", queueNames });
    }

    [HttpGet("{id}/machine-queues")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetMachineQueues(int id)
    {
        var queues = await _profileService.GetMachineQueueNamesAsync(id);
        return Ok(queues);
    }
}
