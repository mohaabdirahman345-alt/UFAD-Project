using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Domain.Enums;
using UnifiedFreelanceArtisansDirectory.Services;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Pages;

// Note: this page lives outside the role-restricted Areas, so it's reachable
// by anyone — including anonymous visitors browsing the public directory.
// Its POST handlers therefore check authentication/role explicitly rather
// than relying on [Authorize], which Razor Pages only enforces at the page
// level (all handlers), not per individual handler method.
public class ServiceProviderDetailsModel : PageModel
{
    private readonly IServiceProviderService _serviceProviderService;
    private readonly IFavoriteService _favoriteService;
    private readonly IReviewService _reviewService;
    private readonly IReportService _reportService;
    private readonly IServiceRequestService _serviceRequestService;
    private readonly IClientProfileService _clientProfileService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ServiceProviderDetailsModel(
        IServiceProviderService serviceProviderService,
        IFavoriteService favoriteService,
        IReviewService reviewService,
        IReportService reportService,
        IServiceRequestService serviceRequestService,
        IClientProfileService clientProfileService,
        UserManager<ApplicationUser> userManager)
    {
        _serviceProviderService = serviceProviderService;
        _favoriteService = favoriteService;
        _reviewService = reviewService;
        _reportService = reportService;
        _serviceRequestService = serviceRequestService;
        _clientProfileService = clientProfileService;
        _userManager = userManager;
    }

    public ServiceProviderDetailViewModel ServiceProvider { get; private set; } = null!;

    public bool IsFavorited { get; private set; }

    public bool CanReview { get; private set; }

    public bool IsOwnProfile { get; private set; }

    [BindProperty]
    public ReviewCreateInput ReviewInput { get; set; } = new();

    [BindProperty]
    public ReportSubmission Report { get; set; } = new();

    [BindProperty]
    public ServiceRequestCreateInput RequestInput { get; set; } = new();

    public class ReportSubmission
    {
        public ReportReason Reason { get; set; }

        public string Details { get; set; } = string.Empty;
    }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var serviceProvider = await _serviceProviderService.GetDetailsAsync(id);
        if (serviceProvider is null)
        {
            return NotFound();
        }

        IsOwnProfile = User.Identity?.IsAuthenticated == true && _userManager.GetUserId(User) == serviceProvider.UserId;

        // Draft/pending/rejected profiles aren't public yet — only the owner
        // or an administrator can preview them before approval.
        if (serviceProvider.ApprovalStatus != ApprovalStatus.Approved && !IsOwnProfile && !User.IsInRole("Administrator"))
        {
            return NotFound();
        }

        if (!IsOwnProfile)
        {
            await _serviceProviderService.IncrementProfileViewsAsync(id);
        }

        ServiceProvider = serviceProvider;

        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Client"))
        {
            var clientProfile = await _clientProfileService.GetByUserIdAsync(_userManager.GetUserId(User)!);
            if (clientProfile is not null)
            {
                IsFavorited = await _favoriteService.IsFavoritedAsync(clientProfile.Id, id);
                CanReview = !await _reviewService.HasClientReviewedAsync(clientProfile.Id, id);
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostToggleFavoriteAsync(int id)
    {
        if (!User.IsInRole("Client"))
        {
            return Challenge();
        }

        var clientProfile = await _clientProfileService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (clientProfile is not null)
        {
            await _favoriteService.ToggleAsync(clientProfile.Id, id);
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostSubmitReviewAsync(int id)
    {
        if (!User.IsInRole("Client"))
        {
            return Challenge();
        }

        var clientProfile = await _clientProfileService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (clientProfile is null)
        {
            return RedirectToPage(new { id });
        }

        ReviewInput.ServiceProviderProfileId = id;

        try
        {
            await _reviewService.AddReviewAsync(clientProfile.Id, ReviewInput);
            StatusMessage = "Thank you — your review has been posted.";
        }
        catch (InvalidOperationException ex)
        {
            StatusMessage = ex.Message;
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostSubmitReportAsync(int id)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Challenge();
        }

        var serviceProvider = await _serviceProviderService.GetDetailsAsync(id);
        if (serviceProvider is null)
        {
            return NotFound();
        }

        await _reportService.SubmitAsync(_userManager.GetUserId(User)!, serviceProvider.UserId, null, Report.Reason, Report.Details);
        StatusMessage = "Thank you — your report has been sent to the administrators.";

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostSubmitRequestAsync(int id)
    {
        if (!User.IsInRole("Client"))
        {
            return Challenge();
        }

        var clientProfile = await _clientProfileService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (clientProfile is null)
        {
            return RedirectToPage(new { id });
        }

        RequestInput.ServiceProviderProfileId = id;

        if (string.IsNullOrWhiteSpace(RequestInput.Title) || string.IsNullOrWhiteSpace(RequestInput.Description))
        {
            StatusMessage = "Please describe what you need before sending the request.";
            return RedirectToPage(new { id });
        }

        await _serviceRequestService.CreateRequestAsync(clientProfile.Id, _userManager.GetUserId(User)!, RequestInput);
        StatusMessage = "Your request has been sent — you can track it from \"My Service Requests\" on your dashboard.";

        return RedirectToPage(new { id });
    }
}
