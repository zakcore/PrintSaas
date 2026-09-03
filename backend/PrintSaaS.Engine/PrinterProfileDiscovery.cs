using Microsoft.Extensions.Logging;
using PrintSaaS.Data.Repositories;
using PrintSaaS.Models;

namespace PrintSaaS.Engine;

public interface IPrinterProfileDiscovery
{
    Task<List<string>> SyncQueueNamesAsync(int printerId);
}

public class PrinterProfileDiscovery : IPrinterProfileDiscovery
{
    private readonly IPrinterRepository _printerRepo;
    private readonly IIppPrintSender _ippSender;
    private readonly ILogger<PrinterProfileDiscovery> _logger;

    public PrinterProfileDiscovery(
        IPrinterRepository printerRepo,
        IIppPrintSender ippSender,
        ILogger<PrinterProfileDiscovery> logger)
    {
        _printerRepo = printerRepo;
        _ippSender = ippSender;
        _logger = logger;
    }

    public async Task<List<string>> SyncQueueNamesAsync(int printerId)
    {
        var printer = await _printerRepo.GetByIdAsync(printerId);
        if (printer is null)
        {
            _logger.LogWarning("Printer {PrinterId} not found for queue sync", printerId);
            return new List<string>();
        }

        var discoveredNames = await _ippSender.DiscoverQueueNamesAsync(printer);

        var machineQueueNames = discoveredNames.Select(name => new MachineQueueName
        {
            PrinterId = printerId,
            QueueName = name,
            DiscoveredAt = DateTime.UtcNow
        }).ToList();

        await _printerRepo.SaveMachineQueueNamesAsync(printerId, machineQueueNames);

        _logger.LogInformation(
            "Synced {Count} queue names for printer {PrinterName}",
            machineQueueNames.Count, printer.Name);

        return discoveredNames;
    }
}
