using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace PrintSaaS.Engine;

public interface IAlertService
{
    Task SendAlertAsync(string subject, string body);
    Task SendPaperLowAlertAsync(string printerName, int trayNumber, int level);
    Task SendJobErrorAlertAsync(string jobName, string error);
}

public class AlertService : IAlertService
{
    private readonly IConfiguration _config;
    private readonly ILogger<AlertService> _logger;

    public AlertService(IConfiguration config, ILogger<AlertService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAlertAsync(string subject, string body)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_config["Mail:From"] ?? "print@sogica.ca"));
            message.To.Add(MailboxAddress.Parse(_config["Mail:AlertRecipient"] ?? "supervisor@sogica.ca"));
            message.Subject = $"[PrintSaaS] {subject}";
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _config["Mail:Host"] ?? "mail.sogica.ca",
                int.Parse(_config["Mail:Port"] ?? "587"),
                MailKit.Security.SecureSocketOptions.StartTls);

            // If credentials are configured
            var mailUser = _config["Mail:Username"];
            var mailPass = _config["Mail:Password"];
            if (!string.IsNullOrEmpty(mailUser))
            {
                await client.AuthenticateAsync(mailUser, mailPass);
            }

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Alert email sent: {Subject}", subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send alert email: {Subject}", subject);
        }
    }

    public async Task SendPaperLowAlertAsync(string printerName, int trayNumber, int level)
    {
        await SendAlertAsync(
            $"Paper Low — {printerName} Tray {trayNumber}",
            $"Printer: {printerName}\n" +
            $"Tray: {trayNumber}\n" +
            $"Paper Level: {level}\n\n" +
            "Please refill the paper tray.\n\n" +
            $"Imprimante : {printerName}\n" +
            $"Plateau : {trayNumber}\n" +
            $"Niveau de papier : {level}\n\n" +
            "Veuillez recharger le plateau de papier.");
    }

    public async Task SendJobErrorAlertAsync(string jobName, string error)
    {
        await SendAlertAsync(
            $"Job Error — {jobName}",
            $"Job: {jobName}\n" +
            $"Error: {error}\n\n" +
            "Please investigate.\n\n" +
            $"Travail : {jobName}\n" +
            $"Erreur : {error}\n\n" +
            "Veuillez vérifier.");
    }
}
