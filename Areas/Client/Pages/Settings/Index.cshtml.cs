using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;

namespace UnifiedFreelanceArtisansDirectory.Areas.Client.Pages.Settings;

public class IndexModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IndexModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [BindProperty]
    public ChangePasswordInput Input { get; set; } = new();

    [BindProperty]
    public IFormFile? AvatarFile { get; set; }

    public string Email { get; private set; } = string.Empty;

    public string? ProfileImagePath { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public class ChangePasswordInput
    {
        [Required, DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 8)]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password), Compare(nameof(NewPassword), ErrorMessage = "The passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        Email = user.Email ?? string.Empty;
        ProfileImagePath = user.ProfileImagePath;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        Email = user.Email ?? string.Empty;
        ProfileImagePath = user.ProfileImagePath;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _userManager.ChangePasswordAsync(user, Input.CurrentPassword, Input.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        await _signInManager.RefreshSignInAsync(user);
        StatusMessage = "Your password has been updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUploadAvatarAsync([FromServices] IWebHostEnvironment environment)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return NotFound();
        }

        Email = user.Email ?? string.Empty;
        ProfileImagePath = user.ProfileImagePath;

        if (AvatarFile is { Length: > 0 })
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(AvatarFile.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
            {
                ModelState.AddModelError("AvatarFile", "Only JPG, PNG, or WEBP images are allowed.");
                return Page();
            }

            var folder = Path.Combine(environment.WebRootPath, "uploads", "avatars");
            Directory.CreateDirectory(folder);
            var fileName = $"avatar-{user.Id}{ext}";
            var fullPath = Path.Combine(folder, fileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await AvatarFile.CopyToAsync(stream);
            }

            user.ProfileImagePath = $"/uploads/avatars/{fileName}";
            await _userManager.UpdateAsync(user);

            StatusMessage = "Your profile picture has been updated!";
            return RedirectToPage();
        }

        ModelState.AddModelError("AvatarFile", "Please select an image file to upload.");
        return Page();
    }
}
