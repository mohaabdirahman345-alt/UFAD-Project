using Microsoft.EntityFrameworkCore;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Domain.Enums;
using UnifiedFreelanceArtisansDirectory.Repositories;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Services;

public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public ReviewService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<bool> HasClientReviewedAsync(int clientProfileId, int serviceProviderProfileId) =>
        (await _unitOfWork.Reviews.FindAsync(r =>
            r.ClientProfileId == clientProfileId && r.ServiceProviderProfileId == serviceProviderProfileId))
        .Any();

    public async Task AddReviewAsync(int clientProfileId, ReviewCreateInput input)
    {
        if (input.Rating is < 1 or > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(input), "Rating must be between 1 and 5.");
        }

        if (await HasClientReviewedAsync(clientProfileId, input.ServiceProviderProfileId))
        {
            throw new InvalidOperationException("You have already reviewed this serviceProvider.");
        }

        var review = new Review
        {
            ClientProfileId = clientProfileId,
            ServiceProviderProfileId = input.ServiceProviderProfileId,
            Rating = input.Rating,
            Comment = input.Comment.Trim()
        };

        await _unitOfWork.Reviews.AddAsync(review);
        await _unitOfWork.SaveChangesAsync();

        await RecalculateRatingAsync(input.ServiceProviderProfileId);
        await _unitOfWork.SaveChangesAsync();

        var provider = await _unitOfWork.ServiceProviders.GetByIdAsync(input.ServiceProviderProfileId);
        var client = await _unitOfWork.Clients.Query()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == clientProfileId);

        if (provider is not null && client is not null)
        {
            await _notificationService.NotifyUserAsync(
                provider.UserId,
                NotificationType.ReviewReceived,
                "New review",
                $"{client.User.FullName} left you a {input.Rating}-star review.",
                "/ServiceProvider/Reviews",
                "Review",
                review.Id);
        }
    }

    public async Task UpdateReviewAsync(int reviewId, int clientProfileId, int rating, string comment)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId)
            ?? throw new InvalidOperationException($"Review {reviewId} was not found.");

        if (review.ClientProfileId != clientProfileId)
        {
            throw new UnauthorizedAccessException("You may only edit your own reviews.");
        }

        review.Rating = rating;
        review.Comment = comment.Trim();
        review.EditedOn = DateTime.UtcNow;

        _unitOfWork.Reviews.Update(review);
        await _unitOfWork.SaveChangesAsync();

        await RecalculateRatingAsync(review.ServiceProviderProfileId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteReviewAsync(int reviewId, int clientProfileId)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId)
            ?? throw new InvalidOperationException($"Review {reviewId} was not found.");

        if (review.ClientProfileId != clientProfileId)
        {
            throw new UnauthorizedAccessException("You may only delete your own reviews.");
        }

        var serviceProviderProfileId = review.ServiceProviderProfileId;

        _unitOfWork.Reviews.Remove(review);
        await _unitOfWork.SaveChangesAsync();

        await RecalculateRatingAsync(serviceProviderProfileId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ReplyAsync(int reviewId, int serviceProviderProfileId, string reply)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId)
            ?? throw new InvalidOperationException($"Review {reviewId} was not found.");

        if (review.ServiceProviderProfileId != serviceProviderProfileId)
        {
            throw new UnauthorizedAccessException("You may only reply to reviews left on your own profile.");
        }

        review.ServiceProviderReply = reply.Trim();
        review.RepliedOn = DateTime.UtcNow;

        _unitOfWork.Reviews.Update(review);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<ReviewDisplayViewModel>> GetForClientAsync(int clientProfileId) =>
        (await _unitOfWork.Reviews.Query()
            .Include(r => r.ServiceProviderProfile)
            .Where(r => r.ClientProfileId == clientProfileId)
            .OrderByDescending(r => r.CreatedOn)
            .ToListAsync())
        .Select(r => new ReviewDisplayViewModel
        {
            Id = r.Id,
            ClientName = r.ServiceProviderProfile.Headline,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedOn = r.CreatedOn,
            ServiceProviderReply = r.ServiceProviderReply,
            RepliedOn = r.RepliedOn
        })
        .ToList();

    public async Task<IReadOnlyList<AdminReviewViewModel>> GetAllForAdminAsync() =>
        (await _unitOfWork.Reviews.Query()
            .Include(r => r.ClientProfile).ThenInclude(c => c.User)
            .Include(r => r.ServiceProviderProfile).ThenInclude(f => f.User)
            .OrderByDescending(r => r.CreatedOn)
            .ToListAsync())
        .Select(r => new AdminReviewViewModel
        {
            Id = r.Id,
            ClientName = r.ClientProfile.User.FullName,
            ServiceProviderName = r.ServiceProviderProfile.User.FullName,
            ServiceProviderProfileId = r.ServiceProviderProfileId,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedOn = r.CreatedOn
        })
        .ToList();

    public async Task<IReadOnlyList<AdminReviewViewModel>> GetFeaturedAsync(int count) =>
        (await _unitOfWork.Reviews.Query()
            .Include(r => r.ClientProfile).ThenInclude(c => c.User)
            .Include(r => r.ServiceProviderProfile).ThenInclude(f => f.User)
            .Where(r => r.Rating >= 4 && r.Comment.Length > 0)
            .OrderByDescending(r => r.CreatedOn)
            .Take(count)
            .ToListAsync())
        .Select(r => new AdminReviewViewModel
        {
            Id = r.Id,
            ClientName = r.ClientProfile.User.FullName,
            ServiceProviderName = r.ServiceProviderProfile.User.FullName,
            ServiceProviderProfileId = r.ServiceProviderProfileId,
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedOn = r.CreatedOn
        })
        .ToList();

    public async Task AdminDeleteAsync(int reviewId)
    {
        var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);
        if (review is null)
        {
            return;
        }

        var serviceProviderProfileId = review.ServiceProviderProfileId;

        _unitOfWork.Reviews.Remove(review);
        await _unitOfWork.SaveChangesAsync();

        await RecalculateRatingAsync(serviceProviderProfileId);
        await _unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Recomputes the denormalized AverageRating/ReviewCount on the
    /// service provider profile. Called after every review add, edit, and delete
    /// so directory listings never read a stale aggregate.
    /// </summary>
    private async Task RecalculateRatingAsync(int serviceProviderProfileId)
    {
        var reviews = await _unitOfWork.Reviews.FindAsync(r => r.ServiceProviderProfileId == serviceProviderProfileId);
        var profile = await _unitOfWork.ServiceProviders.GetByIdAsync(serviceProviderProfileId)
            ?? throw new InvalidOperationException($"ServiceProvider profile {serviceProviderProfileId} was not found.");

        profile.ReviewCount = reviews.Count;
        profile.AverageRating = reviews.Count == 0 ? 0 : Math.Round(reviews.Average(r => r.Rating), 2);

        _unitOfWork.ServiceProviders.Update(profile);
    }
}
