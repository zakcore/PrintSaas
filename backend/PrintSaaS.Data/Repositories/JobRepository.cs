using Microsoft.EntityFrameworkCore;
using PrintSaaS.Models;
using PrintSaaS.Models.Enums;

namespace PrintSaaS.Data.Repositories;

public interface IJobRepository
{
    Task<List<Job>> GetAllAsync(JobStatus? status = null, string? clientName = null);
    Task<Job?> GetByIdAsync(int id);
    Task<Job> CreateAsync(Job job);
    Task UpdateAsync(Job job);
    Task<List<JobHistory>> GetHistoryAsync(int jobId);
    Task AddHistoryAsync(JobHistory history);
}

public class JobRepository : IJobRepository
{
    private readonly PrintContext _context;

    public JobRepository(PrintContext context)
    {
        _context = context;
    }

    public async Task<List<Job>> GetAllAsync(JobStatus? status = null, string? clientName = null)
    {
        var query = _context.Jobs
            .Include(j => j.Printer)
            .Include(j => j.QueueProfile)
            .Include(j => j.Parameters)
            .Include(j => j.Operator)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(j => j.Status == status.Value);

        if (!string.IsNullOrEmpty(clientName))
            query = query.Where(j => j.ClientName.Contains(clientName));

        return await query.OrderByDescending(j => j.CreatedAt).ToListAsync();
    }

    public async Task<Job?> GetByIdAsync(int id)
    {
        return await _context.Jobs
            .Include(j => j.Printer)
            .Include(j => j.QueueProfile)
            .Include(j => j.Parameters)
            .Include(j => j.Operator)
            .Include(j => j.History.OrderByDescending(h => h.Timestamp))
            .FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<Job> CreateAsync(Job job)
    {
        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();
        return job;
    }

    public async Task UpdateAsync(Job job)
    {
        _context.Jobs.Update(job);
        await _context.SaveChangesAsync();
    }

    public async Task<List<JobHistory>> GetHistoryAsync(int jobId)
    {
        return await _context.JobHistories
            .Where(h => h.JobId == jobId)
            .OrderByDescending(h => h.Timestamp)
            .ToListAsync();
    }

    public async Task AddHistoryAsync(JobHistory history)
    {
        _context.JobHistories.Add(history);
        await _context.SaveChangesAsync();
    }
}
