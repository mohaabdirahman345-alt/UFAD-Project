using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Domain.Enums;
using UnifiedFreelanceArtisansDirectory.Services;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Areas.ServiceProvider.Pages.CompletedJobs;

public class IndexModel : PageModel
{
    private readonly IServiceRequestService _serviceRequestService;
    private readonly IServiceProviderService _serviceProviderService;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(IServiceRequestService serviceRequestService, IServiceProviderService serviceProviderService, UserManager<ApplicationUser> userManager)
    {
        _serviceRequestService = serviceRequestService;
        _serviceProviderService = serviceProviderService;
        _userManager = userManager;
    }

    public IReadOnlyList<ServiceRequestSummaryViewModel> CompletedJobs { get; private set; } = new List<ServiceRequestSummaryViewModel>();

    public async Task<IActionResult> OnGetAsync()
    {
        var profile = await _serviceProviderService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (profile is null)
        {
            return NotFound();
        }

        var all = await _serviceRequestService.GetForServiceProviderAsync(profile.Id);
        CompletedJobs = all.Where(r => r.Status == ServiceRequestStatus.Completed).ToList();

        return Page();
    }
}
