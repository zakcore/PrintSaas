using NRules;
using NRules.Fluent;
using PrintSaaS.Models;

namespace PrintSaaS.Rules;

public class PrintRuleResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Warnings { get; } = new();
    public List<string> Errors { get; } = new();
    public int? SuggestedProfileId { get; set; }
    public bool RequiresOperatorConfirmation { get; set; }
    public string? ConfirmationReason { get; set; }
}

public interface IPrintRulesEngine
{
    PrintRuleResult Evaluate(Job job, Printer printer, QueueProfile? profile, Job? lastJobOnPrinter);
}

public class PrintRulesEngine : IPrintRulesEngine
{
    public PrintRuleResult Evaluate(Job job, Printer printer, QueueProfile? profile, Job? lastJobOnPrinter)
    {
        var result = new PrintRuleResult();

        // Rule 1: Color mode enforcement
        EvaluateColorMode(job, printer, result);

        // Rule 2: Payroll page count validation
        EvaluatePayrollPageCount(job, result);

        // Rule 3: Job sequencing (payroll safety)
        EvaluateJobSequencing(job, printer, lastJobOnPrinter, result);

        // Rule 4: Duplex validation
        EvaluateDuplex(job, profile, result);

        return result;
    }

    private static void EvaluateColorMode(Job job, Printer printer, PrintRuleResult result)
    {
        var isColorJob = job.Parameters?.ColorMode == "Color";
        var isBwPrinter = printer.ColorCapability == "BW";

        if (isColorJob && isBwPrinter)
        {
            result.IsValid = false;
            result.Errors.Add(
                $"Cannot send color job to B&W printer '{printer.Name}'. " +
                "Use Brenva HD or Iridesse for color jobs. / " +
                $"Impossible d'envoyer un travail couleur à l'imprimante N&B '{printer.Name}'. " +
                "Utilisez Brenva HD ou Iridesse pour les travaux couleur.");
        }

        if (!isColorJob && printer.ColorCapability == "Both")
        {
            result.Warnings.Add(
                $"B&W job assigned to color printer '{printer.Name}'. Consider using a B&W printer for efficiency. / " +
                $"Travail N&B assigné à l'imprimante couleur '{printer.Name}'. Considérez une imprimante N&B.");
        }
    }

    private static void EvaluatePayrollPageCount(Job job, PrintRuleResult result)
    {
        if (job.Parameters is not { IsPayrollJob: true }) return;

        var p = job.Parameters;
        if (p.EmployeeCount is null || p.PagesPerEmployee is null) return;

        var expectedPages = p.EmployeeCount.Value * p.PagesPerEmployee.Value;
        var actualPages = p.TotalPageCount;

        if (actualPages == 0 || expectedPages == 0) return;

        var difference = Math.Abs(actualPages - expectedPages) / (double)expectedPages;
        if (difference > 0.05)
        {
            result.Warnings.Add(
                $"Page count mismatch: expected {expectedPages} pages ({p.EmployeeCount} employees x {p.PagesPerEmployee} pages), " +
                $"but PDF has {actualPages} pages ({difference:P0} difference). / " +
                $"Différence de pages : attendu {expectedPages} pages ({p.EmployeeCount} employés x {p.PagesPerEmployee} pages), " +
                $"mais le PDF contient {actualPages} pages ({difference:P0} de différence).");
        }
    }

    private static void EvaluateJobSequencing(Job job, Printer printer, Job? lastJob, PrintRuleResult result)
    {
        var isPayrollJob = job.Parameters is { IsPayrollJob: true } or { ContainsCheques: true };
        if (!isPayrollJob) return;
        if (lastJob is null) return;

        var lastWasPayroll = lastJob.Parameters is { IsPayrollJob: true } or { ContainsCheques: true };
        if (lastWasPayroll) return;

        var timeSinceLast = DateTime.UtcNow - (lastJob.CompletedAt ?? lastJob.SentAt ?? DateTime.UtcNow);
        if (timeSinceLast.TotalMinutes >= 30) return;

        result.RequiresOperatorConfirmation = true;
        result.ConfirmationReason =
            $"Last job on '{printer.Name}' was not a payroll/cheque job and completed {timeSinceLast.TotalMinutes:F0} minutes ago. " +
            "Please confirm before sending this payroll job. / " +
            $"Le dernier travail sur '{printer.Name}' n'était pas un travail de paie/chèque et s'est terminé il y a {timeSinceLast.TotalMinutes:F0} minutes. " +
            "Veuillez confirmer avant d'envoyer ce travail de paie.";
    }

    private static void EvaluateDuplex(Job job, QueueProfile? profile, PrintRuleResult result)
    {
        if (profile is null || job.Parameters is null) return;

        if (job.Parameters.IsDuplex != profile.IsDuplex)
        {
            result.Warnings.Add(
                $"Job duplex setting ({(job.Parameters.IsDuplex ? "duplex" : "simplex")}) " +
                $"does not match profile '{profile.DisplayName}' ({(profile.IsDuplex ? "duplex" : "simplex")}). / " +
                $"Le paramètre recto-verso du travail ({(job.Parameters.IsDuplex ? "recto-verso" : "recto")}) " +
                $"ne correspond pas au profil '{profile.DisplayName}' ({(profile.IsDuplex ? "recto-verso" : "recto")}).");
        }
    }
}
