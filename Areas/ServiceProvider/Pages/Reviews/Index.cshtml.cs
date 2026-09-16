using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Services;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Areas.ServiceProvider.Pages.Reviews;

public class IndexModel : PageModel
{
    private readonly IServiceProviderService _serviceProviderService;
    private readonly IReviewService _reviewService;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(IServiceProviderService serviceProviderService, IReviewService reviewService, UserManager<ApplicationUser> userManager)
    {
        _serviceProviderService = serviceProviderService;
        _reviewService = reviewService;
        _userManager = userManager;
    }

    public ServiceProviderProfile Profile { get; private set; } = null!;

    public IReadOnlyList<ReviewDisplayViewModel> ProfileReviews { get; private set; } = new List<ReviewDisplayViewModel>();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var profile = await _serviceProviderService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (profile is null)
        {
            return NotFound();
        }

        // Reviews aren't eagerly loaded on the lightweight GetByUserId
        // projection, so pull the fully detailed view for this page.
        var details = await _serviceProviderService.GetDetailsAsync(profile.Id);
        Profile = profile;
        ProfileReviews = details?.Reviews ?? new List<ReviewDisplayViewModel>();

        return Page();
    }

    public async Task<IActionResult> OnPostReplyAsync(int reviewId, string reply)
    {
        var profile = await _serviceProviderService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (profile is null)
        {
            return NotFound();
        }

        await _reviewService.ReplyAsync(reviewId, profile.Id, reply);
        StatusMessage = "Your reply has been posted.";
        return RedirectToPage();
    }
}
