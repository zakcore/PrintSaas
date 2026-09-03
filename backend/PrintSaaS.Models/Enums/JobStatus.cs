namespace PrintSaaS.Models.Enums;

public enum JobStatus
{
    Received,
    Pending,
    Processing,
    Printing,
    Done,
    Error,
    Retained,
    AwaitingOperatorConfirmation,
    BlockedSecurityViolation
}
