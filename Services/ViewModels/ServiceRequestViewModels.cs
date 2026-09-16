using UnifiedFreelanceArtisansDirectory.Domain.Enums;

namespace UnifiedFreelanceArtisansDirectory.Services.ViewModels;

public class ServiceRequestCreateInput
{
    public int ServiceProviderProfileId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime? PreferredDate { get; set; }
}

/// <summary>Row-level projection used on both the client's and the provider's request lists.</summary>
public class ServiceRequestSummaryViewModel
{
    public int Id { get; set; }

    public string ClientName { get; set; } = string.Empty;

    public int ServiceProviderProfileId { get; set; }

    public string ServiceProviderName { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public DateTime? PreferredDate { get; set; }

    public ServiceRequestStatus Status { get; set; }

    public DateTime CreatedOn { get; set; }
}

/// <summary>Full projection including the status timeline, shown on a request's detail view.</summary>
public class ServiceRequestDetailViewModel
{
    public int Id { get; set; }

    public int ClientProfileId { get; set; }

    public string ClientName { get; set; } = string.Empty;

    public int ServiceProviderProfileId { get; set; }

    public string ServiceProviderName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime? PreferredDate { get; set; }

    public ServiceRequestStatus Status { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime? RespondedOn { get; set; }

    public DateTime? CompletedOn { get; set; }

    public string? DeclineReason { get; set; }

    public string? CancellationReason { get; set; }

    public IReadOnlyList<StatusHistoryEntryViewModel> StatusHistory { get; set; } = new List<StatusHistoryEntryViewModel>();
}

public class StatusHistoryEntryViewModel
{
    public string? FromStatus { get; set; }

    public string ToStatus { get; set; } = string.Empty;

    public DateTime ChangedOn { get; set; }

    public string ChangedByName { get; set; } = string.Empty;

    public string? Notes { get; set; }
}
