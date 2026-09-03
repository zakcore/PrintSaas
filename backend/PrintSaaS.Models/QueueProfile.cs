namespace PrintSaaS.Models;

public class QueueProfile
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string MachineQueueName { get; set; } = string.Empty;
    public int PrinterId { get; set; }
    public bool IsDuplex { get; set; }
    public string ColorMode { get; set; } = "BW";
    public int Priority { get; set; }
    public bool HoldOnArrival { get; set; }
    public bool IsActive { get; set; } = true;
    public int? TrayProfileId { get; set; }

    public Printer Printer { get; set; } = null!;
    public TrayProfile? TrayProfile { get; set; }
}
