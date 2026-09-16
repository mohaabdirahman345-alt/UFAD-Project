using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Services;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Areas.Client.Pages.Requests;

public class IndexModel : PageModel
{
    private readonly IServiceRequestService _serviceRequestService;
    private readonly IClientProfileService _clientProfileService;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(IServiceRequestService serviceRequestService, IClientProfileService clientProfileService, UserManager<ApplicationUser> userManager)
    {
        _serviceRequestService = serviceRequestService;
        _clientProfileService = clientProfileService;
        _userManager = userManager;
    }

    public IReadOnlyList<ServiceRequestSummaryViewModel> Requests { get; private set; } = new List<ServiceRequestSummaryViewModel>();

    public ServiceRequestDetailViewModel? ExpandedRequest { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int? expand)
    {
        var profile = await _clientProfileService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (profile is null)
        {
            return NotFound();
        }

        Requests = await _serviceRequestService.GetForClientAsync(profile.Id);

        if (expand.HasValue)
        {
            ExpandedRequest = await _serviceRequestService.GetDetailsAsync(expand.Value);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync(int requestId, string reason)
    {
        var profile = await _clientProfileService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (profile is null)
        {
            return NotFound();
        }

        try
        {
            await _serviceRequestService.CancelAsync(requestId, profile.Id, _userManager.GetUserId(User)!, reason);
            StatusMessage = "Request cancelled.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException)
        {
            StatusMessage = ex.Message;
        }

        return RedirectToPage();
    }
}
