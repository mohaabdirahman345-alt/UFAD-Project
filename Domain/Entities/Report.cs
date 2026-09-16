using UnifiedFreelanceArtisansDirectory.Domain.Enums;

namespace UnifiedFreelanceArtisansDirectory.Domain.Entities;

/// <summary>
/// A user-submitted flag against a profile or a review, queued for
/// administrator review. ReportedUserId points at the account being
/// flagged; ReportedReviewId is set instead when the report targets a
/// specific review rather than a whole profile.
/// </summary>
public class Report
{
    public int Id { get; set; }

    public string ReporterUserId { get; set; } = string.Empty;

    public ApplicationUser ReporterUser { get; set; } = null!;

    public string ReportedUserId { get; set; } = string.Empty;

    public ApplicationUser ReportedUser { get; set; } = null!;

    public int? ReportedReviewId { get; set; }

    public Review? ReportedReview { get; set; }

    public ReportReason Reason { get; set; }

    public string Details { get; set; } = string.Empty;

    public ReportStatus Status { get; set; } = ReportStatus.Pending;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>Notes left by the administrator who resolved or dismissed the report.</summary>
    public string? ResolutionNotes { get; set; }

    public string? ResolvedByUserId { get; set; }

    public ApplicationUser? ResolvedByUser { get; set; }

    public DateTime? ResolvedOn { get; set; }
}
