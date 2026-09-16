namespace UnifiedFreelanceArtisansDirectory.Domain.Enums;

/// <summary>
/// Where a service provider's profile sits in the administrator review
/// workflow. Only <see cref="Approved"/> profiles appear in public search —
/// this is distinct from <c>IsVerified</c>, which is a separate trust badge
/// an administrator can grant on top of approval.
/// </summary>
public enum ApprovalStatus
{
    /// <summary>Registered but hasn't submitted a complete profile for review yet.</summary>
    Draft = 0,

    /// <summary>Submitted and waiting on an administrator to review.</summary>
    PendingApproval = 1,

    Approved = 2,

    Rejected = 3
}
