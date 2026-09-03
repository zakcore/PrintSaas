using Microsoft.EntityFrameworkCore;
using PrintSaaS.Models;

namespace PrintSaaS.Data.Repositories;

public interface IAuditRepository
{
    Task<List<AuditEntry>> GetAllAsync(int? jobId = null, int skip = 0, int take = 100);
    Task<AuditEntry> CreateAsync(AuditEntry entry);
}

public class AuditRepository : IAuditRepository
{
    private readonly PrintContext _context;

    public AuditRepository(PrintContext context)
    {
        _context = context;
    }

    public async Task<List<AuditEntry>> GetAllAsync(int? jobId = null, int skip = 0, int take = 100)
    {
        var query = _context.AuditEntries
            .Include(a => a.Job)
            .Include(a => a.Operator)
            .AsQueryable();

        if (jobId.HasValue)
            query = query.Where(a => a.JobId == jobId.Value);

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<AuditEntry> CreateAsync(AuditEntry entry)
    {
        entry.Timestamp = DateTime.UtcNow;
        _context.AuditEntries.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }
}
