using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Domain.Enums;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Services;

public interface IServiceProviderService
{
    /// <summary>
    /// The public directory search. Only <see cref="ApprovalStatus.Approved"/>
    /// profiles are ever returned, regardless of the filters applied.
    /// </summary>
    Task<PagedResult<ServiceProviderSummaryViewModel>> SearchAsync(
        string? keyword, int? categoryId, string? address, double? minimumRating,
        AvailabilityStatus? availabilityStatus, int pageNumber, int pageSize);

    Task<ServiceProviderDetailViewModel?> GetDetailsAsync(int serviceProviderProfileId);

    Task<ServiceProviderProfile?> GetByUserIdAsync(string userId);

    Task<IReadOnlyList<ServiceProviderSummaryViewModel>> GetTopRatedAsync(int count);

    /// <summary>Full unfiltered listing for administrator screens — includes Draft/Pending/Rejected profiles.</summary>
    Task<IReadOnlyList<ServiceProviderSummaryViewModel>> GetAllForAdminAsync();

    Task<ServiceProviderProfile> CreateProfileAsync(string userId, ServiceProviderProfileEditInput input);

    Task UpdateProfileAsync(int serviceProviderProfileId, ServiceProviderProfileEditInput input);

    /// <summary>
    /// Sets the verification badge. Revoking verification also removes the
    /// profile from the public directory (<see cref="ApprovalStatus.Rejected"/>).
    /// </summary>
    Task SetVerifiedAsync(int serviceProviderProfileId, bool isVerified, string changedByUserId);

    /// <summary>Moves a Draft or Rejected profile into the PendingApproval queue.</summary>
    Task SubmitForApprovalAsync(int serviceProviderProfileId, string submittedByUserId);

    Task ApproveAsync(int serviceProviderProfileId, string approvedByUserId, string? notes);

    Task RejectAsync(int serviceProviderProfileId, string rejectedByUserId, string? notes);

    Task<IReadOnlyList<StatusHistoryEntryViewModel>> GetApprovalHistoryAsync(int serviceProviderProfileId);

    Task IncrementProfileViewsAsync(int serviceProviderProfileId);

    Task MarkNotificationsViewedAsync(int serviceProviderProfileId);
}
