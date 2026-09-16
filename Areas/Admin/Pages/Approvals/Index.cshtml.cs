using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Domain.Enums;
using UnifiedFreelanceArtisansDirectory.Services;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Areas.Admin.Pages.Approvals;

public class IndexModel : PageModel
{
    private readonly IServiceProviderService _serviceProviderService;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(IServiceProviderService serviceProviderService, UserManager<ApplicationUser> userManager)
    {
        _serviceProviderService = serviceProviderService;
        _userManager = userManager;
    }

    public IReadOnlyList<ServiceProviderSummaryViewModel> PendingProviders { get; private set; } = new List<ServiceProviderSummaryViewModel>();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        var all = await _serviceProviderService.GetAllForAdminAsync();
        PendingProviders = all.Where(p => p.ApprovalStatus == ApprovalStatus.PendingApproval).ToList();
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
}
