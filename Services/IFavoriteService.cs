using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Services;

public interface IFavoriteService
{
    Task<bool> IsFavoritedAsync(int clientProfileId, int serviceProviderProfileId);

    /// <summary>Adds the favorite if it doesn't exist, removes it if it does. Returns the new state.</summary>
    Task<bool> ToggleAsync(int clientProfileId, int serviceProviderProfileId);

    Task<IReadOnlyList<ServiceProviderSummaryViewModel>> GetForClientAsync(int clientProfileId);
}
