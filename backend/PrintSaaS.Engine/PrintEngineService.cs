using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using PrintSaaS.Data;
using PrintSaaS.Data.Repositories;
using PrintSaaS.Models;
using PrintSaaS.Models.Enums;
using PrintSaaS.Rules;
using Microsoft.EntityFrameworkCore;

namespace PrintSaaS.Engine;

public class PrintEngineService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<PrintEngineService> _logger;

    public PrintEngineService(IServiceProvider services, ILogger<PrintEngineService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Print Engine Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingJobsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in print engine loop");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessPendingJobsAsync()
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<PrintContext>();
        var ippSender = scope.ServiceProvider.GetRequiredService<IIppPrintSender>();
        var encryptionService = scope.ServiceProvider.GetRequiredService<IFileEncryptionService>();
        var rulesEngine = scope.ServiceProvider.GetRequiredService<IPrintRulesEngine>();
        var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();

        var pendingJobs = await context.Jobs
            .Include(j => j.Printer)
            .Include(j => j.QueueProfile)
            .Include(j => j.Parameters)
            .Where(j => j.Status == JobStatus.Pending
                     && j.PrinterId != null
                     && j.QueueProfileId != null)
            .OrderBy(j => j.CreatedAt)
            .Take(10)
            .ToListAsync();

        foreach (var job in pendingJobs)
        {
            if (job.Printer is null || job.QueueProfile is null) continue;

            try
            {
                // Get last job on this printer for sequencing check
                var lastJob = await context.Jobs
                    .Include(j => j.Parameters)
                    .Where(j => j.PrinterId == job.PrinterId
                             && j.Id != job.Id
                             && (j.Status == JobStatus.Done || j.Status == JobStatus.Retained))
                    .OrderByDescending(j => j.CompletedAt ?? j.SentAt)
                    .FirstOrDefaultAsync();

                // Run rules engine
                var ruleResult = rulesEngine.Evaluate(job, job.Printer, job.QueueProfile, lastJob);

                if (!ruleResult.IsValid)
                {
                    job.Status = JobStatus.BlockedSecurityViolation;
                    context.JobHistories.Add(new JobHistory
                    {
                        JobId = job.Id,
                        Action = "Blocked by rules engine",
                        Timestamp = DateTime.UtcNow,
                        Details = string.Join("; ", ruleResult.Errors)
                    });
                    await context.SaveChangesAsync();
                    await alertService.SendJobErrorAlertAsync(job.JobName, string.Join("; ", ruleResult.Errors));
                    continue;
                }

                if (ruleResult.RequiresOperatorConfirmation)
                {
                    job.Status = JobStatus.AwaitingOperatorConfirmation;
                    context.JobHistories.Add(new JobHistory
                    {
                        JobId = job.Id,
                        Action = "Awaiting operator confirmation",
                        Timestamp = DateTime.UtcNow,
                        Details = ruleResult.ConfirmationReason
                    });
                    await context.SaveChangesAsync();
                    continue;
                }

                // Decrypt file in memory — never write to disk
                job.Status = JobStatus.Processing;
                await context.SaveChangesAsync();

                using var documentStream = encryptionService.DecryptFileToStream(job.FileLocation);

                // Send via IPPS
                var (success, printerJobId, error) = await ippSender.SendJobAsync(
                    job, job.Printer, job.QueueProfile, documentStream);

                if (success)
                {
                    job.Status = JobStatus.Printing;
                    job.PrinterJobId = printerJobId;
                    job.SentAt = DateTime.UtcNow;

                    context.JobHistories.Add(new JobHistory
                    {
                        JobId = job.Id,
                        Action = "Sent to printer via IPPS",
                        OperatorId = job.OperatorId,
                        Timestamp = DateTime.UtcNow,
                        Details = $"Printer job ID: {printerJobId}"
                    });

                    context.AuditEntries.Add(new AuditEntry
                    {
                        JobId = job.Id,
                        Action = "PrintJobSent",
                        OperatorId = job.OperatorId ?? 0,
                        Timestamp = DateTime.UtcNow,
                        ComplianceNote = $"Sent to {job.Printer.Name} via IPPS, " +
                                        $"profile: {job.QueueProfile.DisplayName}, " +
                                        $"pages: {job.Parameters?.TotalPageCount}"
                    });
                }
                else
                {
                    job.Status = JobStatus.Error;
                    context.JobHistories.Add(new JobHistory
                    {
                        JobId = job.Id,
                        Action = "Print send failed",
                        Timestamp = DateTime.UtcNow,
                        Details = error
                    });
                    await alertService.SendJobErrorAlertAsync(job.JobName, error ?? "Unknown error");
                }

                await context.SaveChangesAsync();

                if (ruleResult.Warnings.Count > 0)
                {
                    _logger.LogWarning("Job {JobName} warnings: {Warnings}",
                        job.JobName, string.Join("; ", ruleResult.Warnings));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process job {JobId}", job.Id);
                job.Status = JobStatus.Error;
                context.JobHistories.Add(new JobHistory
                {
                    JobId = job.Id,
                    Action = "Processing error",
                    Timestamp = DateTime.UtcNow,
                    Details = ex.Message
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
