namespace PrintSaaS.Models;

public class JobHistory
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public string Action { get; set; } = string.Empty;
    public int? OperatorId { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Details { get; set; }

    public Job Job { get; set; } = null!;
    public User? Operator { get; set; }
}
