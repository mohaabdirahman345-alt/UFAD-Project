using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Services;

public interface IClientProfileService
{
    Task<ClientProfile?> GetByUserIdAsync(string userId);

    Task<ClientProfile> CreateProfileAsync(string userId, string neighborhood, string phoneNumber);
}
