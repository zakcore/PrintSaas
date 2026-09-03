namespace PrintSaaS.Models;

public class TrayProfile
{
    public int Id { get; set; }
    public int PrinterId { get; set; }
    public int TrayNumber { get; set; }
    public string PaperType { get; set; } = "Plain";
    public string PaperSize { get; set; } = "Letter";
    public string PaperWeight { get; set; } = "75gsm";
    public bool IsActive { get; set; } = true;

    public Printer Printer { get; set; } = null!;
}
