using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Services;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Areas.Client.Pages.Notifications;

public class IndexModel : PageModel
{
    private readonly INotificationService _notificationService;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(INotificationService notificationService, UserManager<ApplicationUser> userManager)
    {
        _notificationService = notificationService;
        _userManager = userManager;
    }

    public IReadOnlyList<NotificationViewModel> Notifications { get; private set; } = Array.Empty<NotificationViewModel>();

    public async Task OnGetAsync()
    {
        var userId = _userManager.GetUserId(User)!;
        Notifications = await _notificationService.GetForUserAsync(userId);
        await _notificationService.MarkAllReadAsync(userId);
    }
}
