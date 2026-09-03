using PrintSaaS.Data.Repositories;
using PrintSaaS.Models;
using PrintSaaS.Models.Enums;

namespace PrintSaaS.Core.Services;

public interface IJobService
{
    Task<List<Job>> GetAllAsync(JobStatus? status = null, string? clientName = null);
    Task<Job?> GetByIdAsync(int id);
    Task<Job> CreateAsync(Job job);
    Task<bool> SendAsync(int jobId, int operatorId);
    Task<bool> ResendAsync(int jobId, int operatorId);
    Task<bool> ConfirmAsync(int jobId, int operatorId, string? complianceNote);
    Task<List<JobHistory>> GetHistoryAsync(int jobId);
}

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepo;
    private readonly IAuditRepository _auditRepo;

    public JobService(IJobRepository jobRepo, IAuditRepository auditRepo)
    {
        _jobRepo = jobRepo;
        _auditRepo = auditRepo;
    }

    public async Task<List<Job>> GetAllAsync(JobStatus? status = null, string? clientName = null)
    {
        return await _jobRepo.GetAllAsync(status, clientName);
    }

    public async Task<Job?> GetByIdAsync(int id)
    {
        return await _jobRepo.GetByIdAsync(id);
    }

    public async Task<Job> CreateAsync(Job job)
    {
        job.Status = JobStatus.Received;
        job.CreatedAt = DateTime.UtcNow;
        return await _jobRepo.CreateAsync(job);
    }

    public async Task<bool> SendAsync(int jobId, int operatorId)
    {
        var job = await _jobRepo.GetByIdAsync(jobId);
        if (job is null) return false;

        if (job.Status != JobStatus.Received && job.Status != JobStatus.Pending
            && job.Status != JobStatus.AwaitingOperatorConfirmation)
            return false;

        job.Status = JobStatus.Pending;
        job.OperatorId = operatorId;
        job.SentAt = DateTime.UtcNow;
        await _jobRepo.UpdateAsync(job);

        await _jobRepo.AddHistoryAsync(new JobHistory
        {
            JobId = jobId,
            Action = "Sent to print queue",
            OperatorId = operatorId,
            Timestamp = DateTime.UtcNow
        });

        await _auditRepo.CreateAsync(new AuditEntry
        {
            JobId = jobId,
            Action = "JobSent",
            OperatorId = operatorId,
            ComplianceNote = $"Job sent to printer {job.Printer?.Name}, profile {job.QueueProfile?.DisplayName}"
        });

        return true;
    }

    public async Task<bool> ResendAsync(int jobId, int operatorId)
    {
        var job = await _jobRepo.GetByIdAsync(jobId);
        if (job is null || job.Status != JobStatus.Retained) return false;

        job.Status = JobStatus.Pending;
        job.SentAt = DateTime.UtcNow;
        await _jobRepo.UpdateAsync(job);

        await _jobRepo.AddHistoryAsync(new JobHistory
        {
            JobId = jobId,
            Action = "Resent from retain queue",
            OperatorId = operatorId,
            Timestamp = DateTime.UtcNow
        });

        await _auditRepo.CreateAsync(new AuditEntry
        {
            JobId = jobId,
            Action = "JobResent",
            OperatorId = operatorId,
            ComplianceNote = "Resent from retain queue"
        });

        return true;
    }

    public async Task<bool> ConfirmAsync(int jobId, int operatorId, string? complianceNote)
    {
        var job = await _jobRepo.GetByIdAsync(jobId);
        if (job is null || job.Status != JobStatus.AwaitingOperatorConfirmation) return false;

        job.Status = JobStatus.Pending;
        await _jobRepo.UpdateAsync(job);

        await _jobRepo.AddHistoryAsync(new JobHistory
        {
            JobId = jobId,
            Action = "Operator confirmed",
            OperatorId = operatorId,
            Timestamp = DateTime.UtcNow,
            Details = complianceNote
        });

        await _auditRepo.CreateAsync(new AuditEntry
        {
            JobId = jobId,
            Action = "OperatorConfirmation",
            OperatorId = operatorId,
            ComplianceNote = complianceNote ?? "Operator confirmed job release"
        });

        return true;
    }

    public async Task<List<JobHistory>> GetHistoryAsync(int jobId)
    {
        return await _jobRepo.GetHistoryAsync(jobId);
    }
}
