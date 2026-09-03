namespace PrintSaaS.Models;

public class MachineQueueName
{
    public int Id { get; set; }
    public int PrinterId { get; set; }
    public string QueueName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DiscoveredAt { get; set; }

    public Printer Printer { get; set; } = null!;
}
