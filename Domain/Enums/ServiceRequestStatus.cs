namespace UnifiedFreelanceArtisansDirectory.Domain.Enums;

/// <summary>Lifecycle of a client's request for work from a service provider.</summary>
public enum ServiceRequestStatus
{
    Pending = 0,
    Accepted = 1,
    Declined = 2,
    Completed = 3,
    Cancelled = 4
}
