namespace PrintSaaS.Models;

public class Printer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 443;
    public string Protocol { get; set; } = "IPPS";
    public string? DefaultQueueName { get; set; }
    public string ColorCapability { get; set; } = "BW";
    public bool IsActive { get; set; } = true;
    public bool IsOnline { get; set; }
    public string? Status { get; set; }
    public DateTime? LastSeenOnline { get; set; }

    public ICollection<QueueProfile> QueueProfiles { get; set; } = new List<QueueProfile>();
    public ICollection<TrayProfile> TrayProfiles { get; set; } = new List<TrayProfile>();
    public ICollection<MachineQueueName> MachineQueueNames { get; set; } = new List<MachineQueueName>();
}
