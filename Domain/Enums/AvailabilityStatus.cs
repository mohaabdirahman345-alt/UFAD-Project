namespace UnifiedFreelanceArtisansDirectory.Domain.Enums;

/// <summary>
/// A service provider's current capacity to take on new work, set by them
/// on their own profile and used as a client-facing search filter.
/// </summary>
public enum AvailabilityStatus
{
    Available = 0,
    Busy = 1,
    Unavailable = 2
}
