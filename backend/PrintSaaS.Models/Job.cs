using PrintSaaS.Models.Enums;

namespace PrintSaaS.Models;

public class Job
{
    public int Id { get; set; }
    public string JobName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string FileLocation { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public JobStatus Status { get; set; }
    public JobType JobType { get; set; }
    public int? PrinterId { get; set; }
    public int? QueueProfileId { get; set; }
    public int? OperatorId { get; set; }
    public string? PrinterJobId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Printer? Printer { get; set; }
    public QueueProfile? QueueProfile { get; set; }
    public User? Operator { get; set; }
    public JobParameters? Parameters { get; set; }
    public ICollection<JobHistory> History { get; set; } = new List<JobHistory>();
}
