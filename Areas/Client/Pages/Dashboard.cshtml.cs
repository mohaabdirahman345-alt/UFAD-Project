using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Domain.Enums;
using UnifiedFreelanceArtisansDirectory.Services;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Areas.Client.Pages;

public class DashboardModel : PageModel
{
    private readonly IClientProfileService _clientProfileService;
    private readonly IFavoriteService _favoriteService;
    private readonly IReviewService _reviewService;
    private readonly IServiceRequestService _serviceRequestService;
    private readonly IServiceProviderService _serviceProviderService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardModel(
        IClientProfileService clientProfileService,
        IFavoriteService favoriteService,
        IReviewService reviewService,
        IServiceRequestService serviceRequestService,
        IServiceProviderService serviceProviderService,
        UserManager<ApplicationUser> userManager)
    {
        _clientProfileService = clientProfileService;
        _favoriteService = favoriteService;
        _reviewService = reviewService;
        _serviceRequestService = serviceRequestService;
        _serviceProviderService = serviceProviderService;
        _userManager = userManager;
    }

    public IReadOnlyList<ServiceProviderSummaryViewModel> Favorites { get; private set; } = new List<ServiceProviderSummaryViewModel>();

    public IReadOnlyList<ServiceProviderSummaryViewModel> RecommendedProviders { get; private set; } = new List<ServiceProviderSummaryViewModel>();

    public int ReviewCount { get; private set; }

    public int PendingRequestCount { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var profile = await _clientProfileService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (profile is null)
        {
            return NotFound();
        }

        Favorites = await _favoriteService.GetForClientAsync(profile.Id);
        ReviewCount = (await _reviewService.GetForClientAsync(profile.Id)).Count;

        var requests = await _serviceRequestService.GetForClientAsync(profile.Id);
        PendingRequestCount = requests.Count(r => r.Status == ServiceRequestStatus.Pending);

        var favoriteIds = Favorites.Select(f => f.Id).ToHashSet();
        RecommendedProviders = (await _serviceProviderService.GetTopRatedAsync(8))
            .Where(p => !favoriteIds.Contains(p.Id))
            .Take(4)
            .ToList();

        return Page();
    }
}
