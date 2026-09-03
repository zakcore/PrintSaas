using Microsoft.Extensions.Logging;
using PrintSaaS.Data.Repositories;
using PrintSaaS.Models;

namespace PrintSaaS.Engine;

public interface IPrinterMonitor
{
    Task MonitorAllPrintersAsync();
    Task<PrinterStatusInfo> GetPrinterStatusAsync(Printer printer);
}

public class PrinterStatusInfo
{
    public bool IsOnline { get; set; }
    public string? Status { get; set; }
    public Dictionary<int, int> TrayPaperLevels { get; set; } = new();
}

public class PrinterMonitor : IPrinterMonitor
{
    private readonly IPrinterRepository _printerRepo;
    private readonly ILogger<PrinterMonitor> _logger;

    public PrinterMonitor(IPrinterRepository printerRepo, ILogger<PrinterMonitor> logger)
    {
        _printerRepo = printerRepo;
        _logger = logger;
    }

    public async Task MonitorAllPrintersAsync()
    {
        var printers = await _printerRepo.GetAllAsync(includeInactive: false);

        foreach (var printer in printers)
        {
            try
            {
                var status = await GetPrinterStatusAsync(printer);
                printer.IsOnline = status.IsOnline;
                printer.Status = status.Status;
                printer.LastSeenOnline = status.IsOnline ? DateTime.UtcNow : printer.LastSeenOnline;
                await _printerRepo.UpdateAsync(printer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to monitor printer {PrinterName}", printer.Name);
                printer.IsOnline = false;
                printer.Status = "Monitor error";
                await _printerRepo.UpdateAsync(printer);
            }
        }
    }

    public async Task<PrinterStatusInfo> GetPrinterStatusAsync(Printer printer)
    {
        var info = new PrinterStatusInfo();

        try
        {
            // SNMP OID 1.3.6.1.2.1.25.3.5.1.1.1 — hrPrinterStatus
            // 1=other, 2=unknown, 3=idle, 4=printing, 5=warmup
            // SNMP OID 1.3.6.1.2.1.43.8.2.1.10.1.x — paper level per tray

            // Use SNMP to poll the printer
            var community = new SnmpSharpNet.OctetString("public");
            var agentParams = new SnmpSharpNet.AgentParameters(community)
            {
                Version = SnmpSharpNet.SnmpVersion.Ver2
            };

            var agent = new System.Net.IPAddress(
                System.Net.IPAddress.Parse(printer.IpAddress).GetAddressBytes());
            var target = new SnmpSharpNet.UdpTarget(agent, 161, 3000, 1);

            var pdu = new SnmpSharpNet.Pdu(SnmpSharpNet.PduType.Get);
            // hrPrinterStatus
            pdu.VbList.Add("1.3.6.1.2.1.25.3.5.1.1.1");

            var result = await Task.Run(() => target.Request(pdu, agentParams));

            if (result != null && result.Pdu.ErrorStatus == 0)
            {
                info.IsOnline = true;
                var statusValue = result.Pdu.VbList[0].Value.ToString();
                info.Status = statusValue switch
                {
                    "3" => "Idle",
                    "4" => "Printing",
                    "5" => "Warmup",
                    _ => $"Status: {statusValue}"
                };

                // Query tray levels — OID 1.3.6.1.2.1.43.8.2.1.10.1.x
                for (int tray = 1; tray <= 4; tray++)
                {
                    try
                    {
                        var trayPdu = new SnmpSharpNet.Pdu(SnmpSharpNet.PduType.Get);
                        trayPdu.VbList.Add($"1.3.6.1.2.1.43.8.2.1.10.1.{tray}");
                        var trayResult = await Task.Run(() => target.Request(trayPdu, agentParams));
                        if (trayResult?.Pdu.ErrorStatus == 0)
                        {
                            if (int.TryParse(trayResult.Pdu.VbList[0].Value.ToString(), out var level))
                            {
                                info.TrayPaperLevels[tray] = level;
                            }
                        }
                    }
                    catch
                    {
                        // Tray may not exist, skip
                    }
                }
            }
            else
            {
                info.IsOnline = false;
                info.Status = "No SNMP response";
            }

            target.Close();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SNMP query failed for {PrinterName} at {IpAddress}",
                printer.Name, printer.IpAddress);
            info.IsOnline = false;
            info.Status = "Unreachable";
        }

        return info;
    }
}
