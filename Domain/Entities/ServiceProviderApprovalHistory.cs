using UnifiedFreelanceArtisansDirectory.Domain.Enums;

namespace UnifiedFreelanceArtisansDirectory.Domain.Entities;

/// <summary>
/// One recorded transition in a service provider profile's approval
/// lifecycle (Draft -> PendingApproval -> Approved/Rejected, and back again
/// on resubmission). <see cref="ServiceProviderProfile.ApprovalStatus"/>
/// only ever holds the current state — this table is what makes the full
/// history of decisions visible to both the provider and administrators.
/// </summary>
public class ServiceProviderApprovalHistory
{
    public int Id { get; set; }

    public int ServiceProviderProfileId { get; set; }

    public ServiceProviderProfile ServiceProviderProfile { get; set; } = null!;

    public ApprovalStatus? FromStatus { get; set; }

    public ApprovalStatus ToStatus { get; set; }

    public DateTime ChangedOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Null when the provider submitted their own profile — only populated
    /// when an administrator approves or rejects.
    /// </summary>
    public string? ChangedByUserId { get; set; }

    public ApplicationUser? ChangedByUser { get; set; }

    public string? Notes { get; set; }
}
