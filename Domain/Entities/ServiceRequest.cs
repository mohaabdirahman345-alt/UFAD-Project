using UnifiedFreelanceArtisansDirectory.Domain.Enums;

namespace UnifiedFreelanceArtisansDirectory.Domain.Entities;

/// <summary>
/// A client's request for work from a service provider — the booking at the
/// center of the "receive job requests -> accept/decline -> complete
/// service" workflow. Every status change is also written to
/// <see cref="ServiceRequestStatusHistory"/> so the full lifecycle is
/// auditable, not just the current state.
/// </summary>
public class ServiceRequest
{
    public int Id { get; set; }

    public int ClientProfileId { get; set; }

    public ClientProfile ClientProfile { get; set; } = null!;

    public int ServiceProviderProfileId { get; set; }

    public ServiceProviderProfile ServiceProviderProfile { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>When the client would like the work done — a preference, not a guarantee.</summary>
    public DateTime? PreferredDate { get; set; }

    public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.Pending;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    /// <summary>When the provider accepted or declined.</summary>
    public DateTime? RespondedOn { get; set; }

    public DateTime? CompletedOn { get; set; }

    public string? DeclineReason { get; set; }

    public string? CancellationReason { get; set; }

    public ICollection<ServiceRequestStatusHistory> StatusHistory { get; set; } = new List<ServiceRequestStatusHistory>();
}
