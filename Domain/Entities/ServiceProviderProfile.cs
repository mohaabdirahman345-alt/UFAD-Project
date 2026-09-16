using UnifiedFreelanceArtisansDirectory.Domain.Enums;

namespace UnifiedFreelanceArtisansDirectory.Domain.Entities;

/// <summary>
/// The professional profile a service provider presents to the public
/// directory. A single Service Provider profile covers every profession the
/// platform supports (electrician, tailor, web developer, photographer, and
/// so on) — the profession itself is expressed through Category and Skills
/// rather than through a separate account type per trade.
/// </summary>
public class ServiceProviderProfile
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public string Headline { get; set; } = string.Empty;

    public string Bio { get; set; } = string.Empty;

    public int YearsOfExperience { get; set; }

    public decimal? HourlyRate { get; set; }

    public string Address { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>Whether the provider is currently taking on new work — a client-facing search filter.</summary>
    public AvailabilityStatus AvailabilityStatus { get; set; } = AvailabilityStatus.Available;

    /// <summary>
    /// Where this profile sits in the registration → review → public listing
    /// workflow. Only <see cref="Enums.ApprovalStatus.Approved"/> profiles are
    /// returned by directory search.
    /// </summary>
    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Draft;

    public DateTime? SubmittedForApprovalOn { get; set; }

    public DateTime? ApprovalDecisionOn { get; set; }

    public string? ApprovalNotes { get; set; }

    /// <summary>
    /// Set by an administrator after verifying the service provider's identity
    /// and trade credentials. Revoking verification also removes the profile
    /// from the public directory (sets <see cref="ApprovalStatus"/> to Rejected).
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Incremented each time the public profile page is viewed. A simple
    /// popularity signal shown on the provider's own dashboard.
    /// </summary>
    public int ProfileViews { get; set; }

    public DateTime? NotificationsViewedOn { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Denormalized average rating, recalculated by the review service
    /// whenever a review is added, edited, or removed. Storing it avoids an
    /// aggregate query on every directory listing page.
    /// </summary>
    public double AverageRating { get; set; }

    public int ReviewCount { get; set; }

    public ICollection<ServiceProviderSkill> ServiceProviderSkills { get; set; } = new List<ServiceProviderSkill>();

    public ICollection<PortfolioItem> PortfolioItems { get; set; } = new List<PortfolioItem>();

    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public ICollection<Favorite> FavoritedBy { get; set; } = new List<Favorite>();

    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

    public ICollection<ServiceProviderApprovalHistory> ApprovalHistory { get; set; } = new List<ServiceProviderApprovalHistory>();

    /// <summary>
    /// The Service Provider's CV/Resume document. A provider can have zero or one CV.
    /// </summary>
    public ServiceProviderCV? CV { get; set; }
}
