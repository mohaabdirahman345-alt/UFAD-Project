using UnifiedFreelanceArtisansDirectory.Domain.Enums;

namespace UnifiedFreelanceArtisansDirectory.Domain.Entities;

/// <summary>
/// One recorded transition in a <see cref="ServiceRequest"/>'s lifecycle.
/// Written alongside every status change so both the client and the
/// provider can see a full timeline, not just the current status.
/// </summary>
public class ServiceRequestStatusHistory
{
    public int Id { get; set; }

    public int ServiceRequestId { get; set; }

    public ServiceRequest ServiceRequest { get; set; } = null!;

    /// <summary>Null for the very first entry (request created).</summary>
    public ServiceRequestStatus? FromStatus { get; set; }

    public ServiceRequestStatus ToStatus { get; set; }

    public DateTime ChangedOn { get; set; } = DateTime.UtcNow;

    public string ChangedByUserId { get; set; } = string.Empty;

    public ApplicationUser ChangedByUser { get; set; } = null!;

    public string? Notes { get; set; }
}
