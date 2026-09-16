using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Services;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Areas.Admin.Pages.Reviews;

public class IndexModel : PageModel
{
    private readonly IReviewService _reviewService;

    public IndexModel(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    public IReadOnlyList<AdminReviewViewModel> Reviews { get; private set; } = new List<AdminReviewViewModel>();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        Reviews = await _reviewService.GetAllForAdminAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int reviewId)
    {
        await _reviewService.AdminDeleteAsync(reviewId);
        StatusMessage = "Review removed.";
        return RedirectToPage();
    }
}
