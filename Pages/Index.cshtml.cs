using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Domain.Enums;
using UnifiedFreelanceArtisansDirectory.Services;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Pages;

public class IndexModel : PageModel
{
    private readonly IServiceProviderService _serviceProviderService;
    private readonly ICategoryService _categoryService;
    private readonly IReviewService _reviewService;
    private readonly IServiceRequestService _serviceRequestService;

    public IndexModel(
        IServiceProviderService serviceProviderService,
        ICategoryService categoryService,
        IReviewService reviewService,
        IServiceRequestService serviceRequestService)
    {
        _serviceProviderService = serviceProviderService;
        _categoryService = categoryService;
        _reviewService = reviewService;
        _serviceRequestService = serviceRequestService;
    }

    public IReadOnlyList<ServiceProviderSummaryViewModel> FeaturedServiceProviders { get; private set; } = new List<ServiceProviderSummaryViewModel>();

    public IReadOnlyList<Category> Categories { get; private set; } = new List<Category>();

    public IReadOnlyList<AdminReviewViewModel> Testimonials { get; private set; } = new List<AdminReviewViewModel>();

    public int ApprovedProviderCount { get; private set; }

    public int CategoryCount { get; private set; }

    public int CompletedJobCount { get; private set; }

    public double AveragePlatformRating { get; private set; }

    public async Task OnGetAsync()
    {
        FeaturedServiceProviders = await _serviceProviderService.GetTopRatedAsync(4);
        Categories = await _categoryService.GetActiveAsync();
        Testimonials = await _reviewService.GetFeaturedAsync(3);

        var allProviders = await _serviceProviderService.GetAllForAdminAsync();
        var approved = allProviders.Where(p => p.ApprovalStatus == ApprovalStatus.Approved).ToList();
        ApprovedProviderCount = approved.Count;
        CategoryCount = Categories.Count;
        AveragePlatformRating = approved.Count(p => p.ReviewCount > 0) == 0
            ? 0
            : Math.Round(approved.Where(p => p.ReviewCount > 0).Average(p => p.AverageRating), 1);

        var allRequests = await _serviceRequestService.GetAllForAdminAsync();
        CompletedJobCount = allRequests.Count(r => r.Status == ServiceRequestStatus.Completed);
    }
}
