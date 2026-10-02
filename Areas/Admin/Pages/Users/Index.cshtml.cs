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

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

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

    public async Task<IActionResult> OnPostResetPasswordAsync(string userId, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
        {
            ErrorMessage = "Password cannot be empty.";
            return RedirectToPage();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            ErrorMessage = "User not found.";
            return RedirectToPage();
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Contains("Administrator"))
        {
            ErrorMessage = "Security policy: Administrator passwords cannot be reset from this interface.";
            return RedirectToPage();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
        {
            StatusMessage = $"Password for {user.FullName} ({user.Email}) was successfully reset to: {newPassword}";
        }
        else
        {
            ErrorMessage = "Password reset failed: " + string.Join("; ", result.Errors.Select(e => e.Description));
        }

        return RedirectToPage();
    }
}
