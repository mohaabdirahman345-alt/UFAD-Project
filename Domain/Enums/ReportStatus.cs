namespace UnifiedFreelanceArtisansDirectory.Domain.Enums;

/// <summary>
/// Lifecycle of a user-submitted report as it moves through administrator
/// review.
/// </summary>
public enum ReportStatus
{
    Pending = 0,
    UnderReview = 1,
    Resolved = 2,
    Dismissed = 3
}
