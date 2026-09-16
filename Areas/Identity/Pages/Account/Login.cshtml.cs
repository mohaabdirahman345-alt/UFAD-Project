using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;

namespace UnifiedFreelanceArtisansDirectory.Areas.Identity.Pages.Account;

public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            // Honor an explicit return URL (e.g. the profile page a client
            // was viewing before being asked to log in). Otherwise send the
            // user to the dashboard for their role.
            if (returnUrl != Url.Content("~/") && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user is not null)
            {
                if (await _userManager.IsInRoleAsync(user, "Administrator"))
                {
                    return RedirectToPage("/Dashboard", new { area = "Admin" });
                }

                if (await _userManager.IsInRoleAsync(user, "ServiceProvider"))
                {
                    return RedirectToPage("/Dashboard", new { area = "ServiceProvider" });
                }

                if (await _userManager.IsInRoleAsync(user, "Client"))
                {
                    return RedirectToPage("/Dashboard", new { area = "Client" });
                }
            }

            return RedirectToPage("/Index", new { area = "" });
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "This account has been locked due to too many failed attempts. Try again in a few minutes.");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Incorrect email or password.");
        }

        return Page();
    }
}
