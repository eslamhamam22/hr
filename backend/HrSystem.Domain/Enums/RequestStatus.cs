namespace HrSystem.Domain.Enums;

/// <summary>
/// Status of leave and overtime requests
/// </summary>
public enum RequestStatus
{
    Draft = 1,
    Submitted = 2,
    PendingManager = 3,
    PendingHR = 4,
    Approved = 5,
    Rejected = 6,
    Cancelled = 7,
    Withdrawn = 8
}
