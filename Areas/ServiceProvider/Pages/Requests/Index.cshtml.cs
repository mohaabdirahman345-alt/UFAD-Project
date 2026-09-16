using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Services;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Areas.ServiceProvider.Pages.Requests;

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

    public IReadOnlyList<ServiceRequestSummaryViewModel> Requests { get; private set; } = new List<ServiceRequestSummaryViewModel>();

    public ServiceRequestDetailViewModel? ExpandedRequest { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int? expand)
    {
        var profile = await _serviceProviderService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (profile is null)
        {
            return NotFound();
        }

        Requests = await _serviceRequestService.GetForServiceProviderAsync(profile.Id);

        if (expand.HasValue)
        {
            ExpandedRequest = await _serviceRequestService.GetDetailsAsync(expand.Value);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAcceptAsync(int requestId)
    {
        var profile = await _serviceProviderService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (profile is null)
        {
            return NotFound();
        }

        try
        {
            await _serviceRequestService.AcceptAsync(requestId, profile.Id, _userManager.GetUserId(User)!);
            StatusMessage = "Request accepted.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException)
        {
            StatusMessage = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeclineAsync(int requestId, string reason)
    {
        var profile = await _serviceProviderService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (profile is null)
        {
            return NotFound();
        }

        try
        {
            await _serviceRequestService.DeclineAsync(requestId, profile.Id, _userManager.GetUserId(User)!, reason);
            StatusMessage = "Request declined.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException)
        {
            StatusMessage = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCompleteAsync(int requestId)
    {
        var profile = await _serviceProviderService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (profile is null)
        {
            return NotFound();
        }

        try
        {
            await _serviceRequestService.CompleteAsync(requestId, profile.Id, _userManager.GetUserId(User)!);
            StatusMessage = "Marked as completed.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException)
        {
            StatusMessage = ex.Message;
        }

        return RedirectToPage();
    }
}
