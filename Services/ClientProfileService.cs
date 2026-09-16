using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Repositories;

namespace UnifiedFreelanceArtisansDirectory.Services;

public class ClientProfileService : IClientProfileService
{
    private readonly IUnitOfWork _unitOfWork;

    public ClientProfileService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ClientProfile?> GetByUserIdAsync(string userId) =>
        (await _unitOfWork.Clients.FindAsync(c => c.UserId == userId)).FirstOrDefault();

    public async Task<ClientProfile> CreateProfileAsync(string userId, string neighborhood, string phoneNumber)
    {
        var profile = new ClientProfile
        {
            UserId = userId,
            Neighborhood = neighborhood.Trim(),
            PhoneNumber = phoneNumber.Trim()
        };

        await _unitOfWork.Clients.AddAsync(profile);
        await _unitOfWork.SaveChangesAsync();

        return profile;
    }
}
