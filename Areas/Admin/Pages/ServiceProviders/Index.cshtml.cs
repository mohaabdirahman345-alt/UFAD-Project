using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Services;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Areas.Admin.Pages.ServiceProviders;

public class IndexModel : PageModel
{
    private readonly IServiceProviderService _serviceProviderService;
    private readonly IServiceProviderCVService _cvService;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(
        IServiceProviderService serviceProviderService,
        IServiceProviderCVService cvService,
        UserManager<ApplicationUser> userManager)
    {
        _serviceProviderService = serviceProviderService;
        _cvService = cvService;
        _userManager = userManager;
    }

    public IReadOnlyList<ServiceProviderSummaryViewModel> ServiceProviders { get; private set; } = new List<ServiceProviderSummaryViewModel>();

    public int? ExpandedProviderId { get; private set; }

    public IReadOnlyList<StatusHistoryEntryViewModel> ExpandedHistory { get; private set; } = new List<StatusHistoryEntryViewModel>();

    public ServiceProviderCV? ExpandedProviderCV { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync(int? history, int? cvreview)
    {
        ServiceProviders = await _serviceProviderService.GetAllForAdminAsync();

        if (history.HasValue)
        {
            ExpandedProviderId = history.Value;
            ExpandedHistory = await _serviceProviderService.GetApprovalHistoryAsync(history.Value);
        }

        if (cvreview.HasValue)
        {
            ExpandedProviderId = cvreview.Value;
            ExpandedProviderCV = await _cvService.GetByServiceProviderProfileIdAsync(cvreview.Value);
        }
    }

    public async Task<IActionResult> OnPostToggleVerifiedAsync(int serviceProviderProfileId, bool isVerified)
    {
        var granting = !isVerified;
        await _serviceProviderService.SetVerifiedAsync(serviceProviderProfileId, granting, _userManager.GetUserId(User)!);
        StatusMessage = granting
            ? "Provider marked as verified."
            : "Verification revoked. Provider removed from the public directory.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostApproveAsync(int serviceProviderProfileId, string? notes)
    {
        await _serviceProviderService.ApproveAsync(serviceProviderProfileId, _userManager.GetUserId(User)!, notes);
        StatusMessage = "Profile approved and now visible in search.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int serviceProviderProfileId, string? notes)
    {
        await _serviceProviderService.RejectAsync(serviceProviderProfileId, _userManager.GetUserId(User)!, notes);
        StatusMessage = "Profile rejected. The provider can update and resubmit.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDownloadCVAsync(int serviceProviderProfileId)
    {
        var cv = await _cvService.GetByServiceProviderProfileIdAsync(serviceProviderProfileId);
        if (cv is null)
        {
            return NotFound("CV not found.");
        }

        var filePath = await _cvService.GetCVFilePathAsync(cv.Id);
        if (filePath is null || !System.IO.File.Exists(filePath))
        {
            return NotFound("CV file not found on server.");
        }

        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        return File(fileBytes, cv.ContentType, cv.OriginalFileName);
    }
}
