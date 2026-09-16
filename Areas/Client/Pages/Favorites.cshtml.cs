using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Services;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Areas.Client.Pages;

public class FavoritesModel : PageModel
{
    private readonly IClientProfileService _clientProfileService;
    private readonly IFavoriteService _favoriteService;
    private readonly UserManager<ApplicationUser> _userManager;

    public FavoritesModel(IClientProfileService clientProfileService, IFavoriteService favoriteService, UserManager<ApplicationUser> userManager)
    {
        _clientProfileService = clientProfileService;
        _favoriteService = favoriteService;
        _userManager = userManager;
    }

    public IReadOnlyList<ServiceProviderSummaryViewModel> Favorites { get; private set; } = new List<ServiceProviderSummaryViewModel>();

    public async Task<IActionResult> OnGetAsync()
    {
        var profile = await _clientProfileService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (profile is null)
        {
            return NotFound();
        }

        Favorites = await _favoriteService.GetForClientAsync(profile.Id);
        return Page();
    }

    public async Task<IActionResult> OnPostRemoveAsync(int serviceProviderProfileId)
    {
        var profile = await _clientProfileService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (profile is null)
        {
            return NotFound();
        }

        await _favoriteService.ToggleAsync(profile.Id, serviceProviderProfileId);
        return RedirectToPage();
    }
}
