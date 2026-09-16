using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Repositories;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Services;

public class PortfolioService : IPortfolioService
{
    private readonly IUnitOfWork _unitOfWork;

    public PortfolioService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PortfolioItemViewModel> AddAsync(int serviceProviderProfileId, string title, string description, string imagePath, DateTime completedOn)
    {
        var existingCount = (await _unitOfWork.PortfolioItems.FindAsync(p => p.ServiceProviderProfileId == serviceProviderProfileId)).Count;

        var item = new PortfolioItem
        {
            ServiceProviderProfileId = serviceProviderProfileId,
            Title = title.Trim(),
            Description = description.Trim(),
            ImagePath = imagePath,
            CompletedOn = completedOn,
            DisplayOrder = existingCount
        };

        await _unitOfWork.PortfolioItems.AddAsync(item);
        await _unitOfWork.SaveChangesAsync();

        return new PortfolioItemViewModel
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            ImagePath = item.ImagePath,
            CompletedOn = item.CompletedOn
        };
    }

    public async Task UpdateAsync(int portfolioItemId, int serviceProviderProfileId, string title, string description, DateTime completedOn)
    {
        var item = await _unitOfWork.PortfolioItems.GetByIdAsync(portfolioItemId)
            ?? throw new InvalidOperationException($"Portfolio item {portfolioItemId} was not found.");

        if (item.ServiceProviderProfileId != serviceProviderProfileId)
        {
            throw new UnauthorizedAccessException("You may only edit items on your own portfolio.");
        }

        item.Title = title.Trim();
        item.Description = description.Trim();
        item.CompletedOn = completedOn;

        _unitOfWork.PortfolioItems.Update(item);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(int portfolioItemId, int serviceProviderProfileId)
    {
        var item = await _unitOfWork.PortfolioItems.GetByIdAsync(portfolioItemId)
            ?? throw new InvalidOperationException($"Portfolio item {portfolioItemId} was not found.");

        if (item.ServiceProviderProfileId != serviceProviderProfileId)
        {
            throw new UnauthorizedAccessException("You may only delete items on your own portfolio.");
        }

        _unitOfWork.PortfolioItems.Remove(item);
        await _unitOfWork.SaveChangesAsync();
    }
}
