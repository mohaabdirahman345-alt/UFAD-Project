using Microsoft.EntityFrameworkCore;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Repositories;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Services;

public class FavoriteService : IFavoriteService
{
    private readonly IUnitOfWork _unitOfWork;

    public FavoriteService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> IsFavoritedAsync(int clientProfileId, int serviceProviderProfileId) =>
        (await _unitOfWork.Favorites.FindAsync(f =>
            f.ClientProfileId == clientProfileId && f.ServiceProviderProfileId == serviceProviderProfileId))
        .Any();

    public async Task<bool> ToggleAsync(int clientProfileId, int serviceProviderProfileId)
    {
        var existing = (await _unitOfWork.Favorites.FindAsync(f =>
            f.ClientProfileId == clientProfileId && f.ServiceProviderProfileId == serviceProviderProfileId))
            .FirstOrDefault();

        if (existing is not null)
        {
            _unitOfWork.Favorites.Remove(existing);
            await _unitOfWork.SaveChangesAsync();
            return false;
        }

        await _unitOfWork.Favorites.AddAsync(new Favorite
        {
            ClientProfileId = clientProfileId,
            ServiceProviderProfileId = serviceProviderProfileId
        });
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<ServiceProviderSummaryViewModel>> GetForClientAsync(int clientProfileId) =>
        (await _unitOfWork.Favorites.Query()
            .Include(f => f.ServiceProviderProfile).ThenInclude(fp => fp.User)
            .Include(f => f.ServiceProviderProfile).ThenInclude(fp => fp.Category)
            .Include(f => f.ServiceProviderProfile).ThenInclude(fp => fp.ServiceProviderSkills).ThenInclude(fs => fs.Skill)
            .Where(f => f.ClientProfileId == clientProfileId)
            .OrderByDescending(f => f.SavedOn)
            .ToListAsync())
        .Select(f => new ServiceProviderSummaryViewModel
        {
            Id = f.ServiceProviderProfile.Id,
            FullName = f.ServiceProviderProfile.User.FullName,
            ProfileImagePath = f.ServiceProviderProfile.User.ProfileImagePath,
            Headline = f.ServiceProviderProfile.Headline,
            CategoryName = f.ServiceProviderProfile.Category.Name,
            Address = f.ServiceProviderProfile.Address,
            HourlyRate = f.ServiceProviderProfile.HourlyRate,
            IsVerified = f.ServiceProviderProfile.IsVerified,
            ApprovalStatus = f.ServiceProviderProfile.ApprovalStatus,
            AvailabilityStatus = f.ServiceProviderProfile.AvailabilityStatus,
            AverageRating = f.ServiceProviderProfile.AverageRating,
            ReviewCount = f.ServiceProviderProfile.ReviewCount,
            TopSkills = f.ServiceProviderProfile.ServiceProviderSkills.Take(3).Select(fs => fs.Skill.Name).ToList()
        })
        .ToList();
}
