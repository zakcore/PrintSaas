using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using PrintSaaS.Data;
using PrintSaaS.Models.Enums;

namespace PrintSaaS.Engine;

public interface IJobProcessor
{
    Task CheckPaperLevelsAsync();
    Task CheckJobErrorsAsync();
    Task CleanupRetainQueueAsync();
}

public class JobProcessor : IJobProcessor
{
    private readonly IServiceProvider _services;
    private readonly ILogger<JobProcessor> _logger;

    public JobProcessor(IServiceProvider services, ILogger<JobProcessor> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task CheckPaperLevelsAsync()
    {
        using var scope = _services.CreateScope();
        var monitor = scope.ServiceProvider.GetRequiredService<IPrinterMonitor>();
        var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();
        var context = scope.ServiceProvider.GetRequiredService<PrintContext>();

        var printers = await context.Printers.Where(p => p.IsActive).ToListAsync();

        foreach (var printer in printers)
        {
            var status = await monitor.GetPrinterStatusAsync(printer);
            foreach (var (tray, level) in status.TrayPaperLevels)
            {
                if (level < 100) // Low paper threshold
                {
                    _logger.LogWarning("Low paper on {Printer} tray {Tray}: {Level}",
                        printer.Name, tray, level);
                    await alertService.SendPaperLowAlertAsync(printer.Name, tray, level);
                }
            }
        }
    }

    public async Task CheckJobErrorsAsync()
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PrintContext>();
        var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();

        var stuckJobs = await context.Jobs
            .Where(j => j.Status == JobStatus.Processing
                     && j.SentAt != null
                     && j.SentAt < DateTime.UtcNow.AddMinutes(-15))
            .ToListAsync();

        foreach (var job in stuckJobs)
        {
            _logger.LogWarning("Job {JobId} appears stuck in Processing state", job.Id);
            await alertService.SendJobErrorAlertAsync(
                job.JobName, "Job stuck in Processing state for > 15 minutes");
        }

        var errorJobs = await context.Jobs
            .Where(j => j.Status == JobStatus.Error
                     && j.SentAt != null
                     && j.SentAt > DateTime.UtcNow.AddMinutes(-30))
            .ToListAsync();

        foreach (var job in errorJobs)
        {
            _logger.LogWarning("Recent error job: {JobId} - {JobName}", job.Id, job.JobName);
        }
    }

    public async Task CleanupRetainQueueAsync()
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PrintContext>();

        var cutoff = DateTime.UtcNow.AddDays(-30);
        var oldRetainedJobs = await context.Jobs
            .Where(j => j.Status == JobStatus.Retained && j.CompletedAt < cutoff)
            .ToListAsync();

        foreach (var job in oldRetainedJobs)
        {
            job.Status = JobStatus.Done;
            context.JobHistories.Add(new Models.JobHistory
            {
                JobId = job.Id,
                Action = "Archived from retain queue",
                Timestamp = DateTime.UtcNow,
                Details = "Auto-archived after 30 days"
            });
        }

        if (oldRetainedJobs.Count > 0)
        {
            await context.SaveChangesAsync();
            _logger.LogInformation("Archived {Count} jobs from retain queue", oldRetainedJobs.Count);
        }
    }
}
