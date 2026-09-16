using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Services;

public interface IPortfolioService
{
    Task<PortfolioItemViewModel> AddAsync(int serviceProviderProfileId, string title, string description, string imagePath, DateTime completedOn);

    Task UpdateAsync(int portfolioItemId, int serviceProviderProfileId, string title, string description, DateTime completedOn);

    Task DeleteAsync(int portfolioItemId, int serviceProviderProfileId);
}
