using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;

namespace UnifiedFreelanceArtisansDirectory.Areas.Admin.Pages.Users;

public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public List<UserRow> Users { get; private set; } = new();

    public record UserRow(string Id, string FullName, string Email, string Role, bool IsActive);

    public async Task OnGetAsync()
    {
        var users = _userManager.Users.OrderBy(u => u.LastName).ToList();
        var rows = new List<UserRow>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            rows.Add(new UserRow(user.Id, user.FullName, user.Email ?? string.Empty, roles.FirstOrDefault() ?? "—", user.IsActive));
        }

        Users = rows;
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(string userId, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is not null)
        {
            user.IsActive = !isActive;
            await _userManager.UpdateAsync(user);
        }

        return RedirectToPage();
    }
}
