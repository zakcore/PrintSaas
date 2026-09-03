namespace PrintSaaS.Models;

public class JobParameters
{
    public int Id { get; set; }
    public int JobId { get; set; }
    public bool IsDuplex { get; set; }
    public string ColorMode { get; set; } = "BW";
    public int Copies { get; set; } = 1;
    public int TotalPageCount { get; set; }
    public bool IsPayrollJob { get; set; }
    public int? EmployeeCount { get; set; }
    public int? PagesPerEmployee { get; set; }
    public bool ContainsCheques { get; set; }
    public bool ContainsStubs { get; set; }
    public string PaperType { get; set; } = "Plain";

    public Job Job { get; set; } = null!;
}
