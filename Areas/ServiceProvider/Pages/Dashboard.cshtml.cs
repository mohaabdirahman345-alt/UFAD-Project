using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Domain.Enums;
using UnifiedFreelanceArtisansDirectory.Services;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Areas.ServiceProvider.Pages;

public class DashboardModel : PageModel
{
    private readonly IServiceProviderService _serviceProviderService;
    private readonly IServiceRequestService _serviceRequestService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardModel(IServiceProviderService serviceProviderService, IServiceRequestService serviceRequestService, UserManager<ApplicationUser> userManager)
    {
        _serviceProviderService = serviceProviderService;
        _serviceRequestService = serviceRequestService;
        _userManager = userManager;
    }

    public ServiceProviderDetailViewModel? Profile { get; private set; }

    public int PendingRequestCount { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = _userManager.GetUserId(User)!;
        var basicProfile = await _serviceProviderService.GetByUserIdAsync(userId);
        if (basicProfile is null)
        {
            return NotFound();
        }

        Profile = await _serviceProviderService.GetDetailsAsync(basicProfile.Id);

        var requests = await _serviceRequestService.GetForServiceProviderAsync(basicProfile.Id);
        PendingRequestCount = requests.Count(r => r.Status == ServiceRequestStatus.Pending);

        return Page();
    }
}
