using Microsoft.Extensions.Logging;
using SharpIpp;
using SharpIpp.Models;
using SharpIpp.Protocol.Models;
using PrintSaaS.Models;

namespace PrintSaaS.Engine;

public interface IIppPrintSender
{
    Task<(bool success, string? printerJobId, string? error)> SendJobAsync(
        Job job, Printer printer, QueueProfile profile, Stream documentStream);
    Task<(bool success, string? error)> TestConnectionAsync(Printer printer);
    Task<List<string>> DiscoverQueueNamesAsync(Printer printer);
}

public class IppPrintSender : IIppPrintSender
{
    private readonly ILogger<IppPrintSender> _logger;

    public IppPrintSender(ILogger<IppPrintSender> logger)
    {
        _logger = logger;
    }

    public async Task<(bool success, string? printerJobId, string? error)> SendJobAsync(
        Job job, Printer printer, QueueProfile profile, Stream documentStream)
    {
        try
        {
            // ALWAYS use MachineQueueName in the IPP URI — never DisplayName
            var printerUri = new Uri(
                $"ipps://{printer.IpAddress}:{printer.Port}/printers/{profile.MachineQueueName}");

            using var client = new SharpIppClient();
            var request = new PrintJobRequest
            {
                PrinterUri = printerUri,
                Document = documentStream,
                NewJobAttributes = new NewJobAttributes
                {
                    JobName = job.JobName,
                    Sides = profile.IsDuplex
                        ? Sides.TwoSidedLongEdge
                        : Sides.OneSided,
                    Copies = job.Parameters?.Copies ?? 1,
                }
            };

            var response = await client.PrintJobAsync(request);

            if (response.StatusCode == IppStatusCode.SuccessfulOk ||
                response.StatusCode == IppStatusCode.SuccessfulOkIgnoredOrSubstitutedAttributes)
            {
                var jobId = response.JobId.ToString();
                _logger.LogInformation(
                    "Job {JobName} sent to {PrinterName} via IPPS, printer job ID: {PrinterJobId}",
                    job.JobName, printer.Name, jobId);
                return (true, jobId, null);
            }

            var error = $"IPP error: {response.StatusCode}";
            _logger.LogError("Failed to send job {JobName}: {Error}", job.JobName, error);
            return (false, null, error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception sending job {JobName} to printer {PrinterName}",
                job.JobName, printer.Name);
            return (false, null, ex.Message);
        }
    }

    public async Task<(bool success, string? error)> TestConnectionAsync(Printer printer)
    {
        try
        {
            var queueName = printer.DefaultQueueName ?? "ipp/print";
            var printerUri = new Uri(
                $"ipps://{printer.IpAddress}:{printer.Port}/printers/{queueName}");

            using var client = new SharpIppClient();
            var request = new GetPrinterAttributesRequest { PrinterUri = printerUri };
            var response = await client.GetPrinterAttributesAsync(request);

            if (response.StatusCode == IppStatusCode.SuccessfulOk)
            {
                _logger.LogInformation("Connection test successful for {PrinterName}", printer.Name);
                return (true, null);
            }

            return (false, $"Printer responded with: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection test failed for {PrinterName}", printer.Name);
            return (false, ex.Message);
        }
    }

    public async Task<List<string>> DiscoverQueueNamesAsync(Printer printer)
    {
        var queueNames = new List<string>();
        try
        {
            var printerUri = new Uri(
                $"ipps://{printer.IpAddress}:{printer.Port}/printers/{printer.DefaultQueueName ?? "ipp/print"}");

            using var client = new SharpIppClient();
            var request = new GetPrinterAttributesRequest { PrinterUri = printerUri };
            var response = await client.GetPrinterAttributesAsync(request);

            // Attempt to extract queue names from printer attributes
            // DFE controllers expose queues as printer-uri-supported attributes
            foreach (var section in response.Sections)
            {
                foreach (var attr in section.Attributes)
                {
                    if (attr.Tag == Tag.Uri && attr.Value is string uri)
                    {
                        // Extract queue name from URI like ipps://host/printers/QueueName
                        var parts = uri.Split("/printers/", StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 1)
                        {
                            queueNames.Add(parts[^1]);
                        }
                    }
                }
            }

            _logger.LogInformation("Discovered {Count} queue names from {PrinterName}",
                queueNames.Count, printer.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to discover queues from {PrinterName}", printer.Name);
        }

        return queueNames;
    }
}
