using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Services;

public interface IReviewService
{
    Task<bool> HasClientReviewedAsync(int clientProfileId, int serviceProviderProfileId);

    Task AddReviewAsync(int clientProfileId, ReviewCreateInput input);

    Task UpdateReviewAsync(int reviewId, int clientProfileId, int rating, string comment);

    Task DeleteReviewAsync(int reviewId, int clientProfileId);

    Task ReplyAsync(int reviewId, int serviceProviderProfileId, string reply);

    Task<IReadOnlyList<ReviewDisplayViewModel>> GetForClientAsync(int clientProfileId);

    /// <summary>Full read-only listing for the admin reviews oversight page.</summary>
    Task<IReadOnlyList<AdminReviewViewModel>> GetAllForAdminAsync();

    /// <summary>A handful of strong, commented reviews for the public landing page's testimonials section.</summary>
    Task<IReadOnlyList<AdminReviewViewModel>> GetFeaturedAsync(int count);

    /// <summary>Administrator removal of a review — separate from the client's own DeleteReviewAsync, which enforces ownership.</summary>
    Task AdminDeleteAsync(int reviewId);
}
