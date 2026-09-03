using PrintSaaS.Data.Repositories;
using PrintSaaS.Models;

namespace PrintSaaS.Core.Services;

public interface IPrinterService
{
    Task<List<Printer>> GetAllAsync(bool includeInactive = false);
    Task<Printer?> GetByIdAsync(int id);
    Task<Printer> CreateAsync(Printer printer);
    Task<bool> UpdateAsync(int id, Printer updated);
    Task<bool> DeactivateAsync(int id);
}

public class PrinterService : IPrinterService
{
    private readonly IPrinterRepository _repo;

    public PrinterService(IPrinterRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<Printer>> GetAllAsync(bool includeInactive = false)
    {
        return await _repo.GetAllAsync(includeInactive);
    }

    public async Task<Printer?> GetByIdAsync(int id)
    {
        return await _repo.GetByIdAsync(id);
    }

    public async Task<Printer> CreateAsync(Printer printer)
    {
        printer.IsActive = true;
        return await _repo.CreateAsync(printer);
    }

    public async Task<bool> UpdateAsync(int id, Printer updated)
    {
        var printer = await _repo.GetByIdAsync(id);
        if (printer is null) return false;

        printer.Name = updated.Name;
        printer.Model = updated.Model;
        printer.IpAddress = updated.IpAddress;
        printer.Port = updated.Port;
        printer.Protocol = updated.Protocol;
        printer.DefaultQueueName = updated.DefaultQueueName;
        printer.ColorCapability = updated.ColorCapability;

        await _repo.UpdateAsync(printer);
        return true;
    }

    public async Task<bool> DeactivateAsync(int id)
    {
        var printer = await _repo.GetByIdAsync(id);
        if (printer is null) return false;

        printer.IsActive = false;
        await _repo.UpdateAsync(printer);
        return true;
    }
}
