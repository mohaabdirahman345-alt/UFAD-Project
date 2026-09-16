using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Domain.Enums;

namespace UnifiedFreelanceArtisansDirectory.Repositories;

/// <summary>
/// Search and detail queries specific to service provider profiles — these need
/// eager loading and filtering that don't belong on the generic contract.
/// </summary>
public interface IServiceProviderRepository : IGenericRepository<ServiceProviderProfile>
{
    /// <summary>
    /// Runs the public directory search: optional keyword (matched against
    /// headline/bio/skills), category, address, minimum rating, and
    /// availability, sorted verified-and-highest-rated first. Only
    /// <see cref="ApprovalStatus.Approved"/> profiles are ever returned.
    /// Pagination keeps large result sets off a single page.
    /// </summary>
    Task<(IReadOnlyList<ServiceProviderProfile> Results, int TotalCount)> SearchAsync(
        string? keyword,
        int? categoryId,
        string? address,
        double? minimumRating,
        AvailabilityStatus? availabilityStatus,
        int pageNumber,
        int pageSize);

    Task<ServiceProviderProfile?> GetProfileWithDetailsAsync(int serviceProviderProfileId);

    Task<ServiceProviderProfile?> GetProfileByUserIdAsync(string userId);

    Task<IReadOnlyList<ServiceProviderProfile>> GetTopRatedAsync(int count);

    /// <summary>Full unpaginated listing for administrator screens (e.g. the approval queue) — includes every approval status.</summary>
    Task<IReadOnlyList<ServiceProviderProfile>> GetAllForAdminAsync();
}
