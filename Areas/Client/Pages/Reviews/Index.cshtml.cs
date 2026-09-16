using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Services;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Areas.Client.Pages.Reviews;

public class IndexModel : PageModel
{
    private readonly IClientProfileService _clientProfileService;
    private readonly IReviewService _reviewService;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(IClientProfileService clientProfileService, IReviewService reviewService, UserManager<ApplicationUser> userManager)
    {
        _clientProfileService = clientProfileService;
        _reviewService = reviewService;
        _userManager = userManager;
    }

    public IReadOnlyList<ReviewDisplayViewModel> MyReviews { get; private set; } = new List<ReviewDisplayViewModel>();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var profile = await _clientProfileService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (profile is null)
        {
            return NotFound();
        }

        MyReviews = await _reviewService.GetForClientAsync(profile.Id);
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int reviewId, int rating, string comment)
    {
        var profile = await _clientProfileService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (profile is null)
        {
            return NotFound();
        }

        await _reviewService.UpdateReviewAsync(reviewId, profile.Id, rating, comment);
        StatusMessage = "Your review has been updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int reviewId)
    {
        var profile = await _clientProfileService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (profile is null)
        {
            return NotFound();
        }

        await _reviewService.DeleteReviewAsync(reviewId, profile.Id);
        StatusMessage = "Your review has been removed.";
        return RedirectToPage();
    }
}
