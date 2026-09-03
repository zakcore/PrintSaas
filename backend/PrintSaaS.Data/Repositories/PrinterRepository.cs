using Microsoft.EntityFrameworkCore;
using PrintSaaS.Models;

namespace PrintSaaS.Data.Repositories;

public interface IPrinterRepository
{
    Task<List<Printer>> GetAllAsync(bool includeInactive = false);
    Task<Printer?> GetByIdAsync(int id);
    Task<Printer> CreateAsync(Printer printer);
    Task UpdateAsync(Printer printer);
    Task<List<QueueProfile>> GetQueueProfilesAsync(int printerId, bool activeOnly = true);
    Task<QueueProfile?> GetQueueProfileByIdAsync(int id);
    Task<QueueProfile> CreateQueueProfileAsync(QueueProfile profile);
    Task UpdateQueueProfileAsync(QueueProfile profile);
    Task<List<TrayProfile>> GetTrayProfilesAsync(int printerId, bool activeOnly = true);
    Task<TrayProfile?> GetTrayProfileByIdAsync(int id);
    Task<TrayProfile> CreateTrayProfileAsync(TrayProfile profile);
    Task UpdateTrayProfileAsync(TrayProfile profile);
    Task<List<MachineQueueName>> GetMachineQueueNamesAsync(int printerId);
    Task SaveMachineQueueNamesAsync(int printerId, List<MachineQueueName> names);
}

public class PrinterRepository : IPrinterRepository
{
    private readonly PrintContext _context;

    public PrinterRepository(PrintContext context)
    {
        _context = context;
    }

    public async Task<List<Printer>> GetAllAsync(bool includeInactive = false)
    {
        var query = _context.Printers
            .Include(p => p.QueueProfiles.Where(q => includeInactive || q.IsActive))
            .Include(p => p.TrayProfiles.Where(t => includeInactive || t.IsActive))
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        return await query.OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<Printer?> GetByIdAsync(int id)
    {
        return await _context.Printers
            .Include(p => p.QueueProfiles)
            .Include(p => p.TrayProfiles)
            .Include(p => p.MachineQueueNames)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Printer> CreateAsync(Printer printer)
    {
        _context.Printers.Add(printer);
        await _context.SaveChangesAsync();
        return printer;
    }

    public async Task UpdateAsync(Printer printer)
    {
        _context.Printers.Update(printer);
        await _context.SaveChangesAsync();
    }

    public async Task<List<QueueProfile>> GetQueueProfilesAsync(int printerId, bool activeOnly = true)
    {
        var query = _context.QueueProfiles
            .Include(q => q.TrayProfile)
            .Where(q => q.PrinterId == printerId);

        if (activeOnly)
            query = query.Where(q => q.IsActive);

        return await query.OrderBy(q => q.Priority).ToListAsync();
    }

    public async Task<QueueProfile?> GetQueueProfileByIdAsync(int id)
    {
        return await _context.QueueProfiles
            .Include(q => q.Printer)
            .Include(q => q.TrayProfile)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<QueueProfile> CreateQueueProfileAsync(QueueProfile profile)
    {
        _context.QueueProfiles.Add(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    public async Task UpdateQueueProfileAsync(QueueProfile profile)
    {
        _context.QueueProfiles.Update(profile);
        await _context.SaveChangesAsync();
    }

    public async Task<List<TrayProfile>> GetTrayProfilesAsync(int printerId, bool activeOnly = true)
    {
        var query = _context.TrayProfiles.Where(t => t.PrinterId == printerId);

        if (activeOnly)
            query = query.Where(t => t.IsActive);

        return await query.OrderBy(t => t.TrayNumber).ToListAsync();
    }

    public async Task<TrayProfile?> GetTrayProfileByIdAsync(int id)
    {
        return await _context.TrayProfiles
            .Include(t => t.Printer)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TrayProfile> CreateTrayProfileAsync(TrayProfile profile)
    {
        _context.TrayProfiles.Add(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    public async Task UpdateTrayProfileAsync(TrayProfile profile)
    {
        _context.TrayProfiles.Update(profile);
        await _context.SaveChangesAsync();
    }

    public async Task<List<MachineQueueName>> GetMachineQueueNamesAsync(int printerId)
    {
        return await _context.MachineQueueNames
            .Where(m => m.PrinterId == printerId)
            .OrderBy(m => m.QueueName)
            .ToListAsync();
    }

    public async Task SaveMachineQueueNamesAsync(int printerId, List<MachineQueueName> names)
    {
        var existing = await _context.MachineQueueNames
            .Where(m => m.PrinterId == printerId)
            .ToListAsync();

        _context.MachineQueueNames.RemoveRange(existing);

        foreach (var name in names)
        {
            name.PrinterId = printerId;
            name.DiscoveredAt = DateTime.UtcNow;
        }

        _context.MachineQueueNames.AddRange(names);
        await _context.SaveChangesAsync();
    }
}
