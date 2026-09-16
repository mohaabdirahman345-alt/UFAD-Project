using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Services;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IServiceProviderService _serviceProviderService;
    private readonly IClientProfileService _clientProfileService;
    private readonly ICategoryService _categoryService;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IServiceProviderService serviceProviderService,
        IClientProfileService clientProfileService,
        ICategoryService categoryService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _serviceProviderService = serviceProviderService;
        _clientProfileService = clientProfileService;
        _categoryService = categoryService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IReadOnlyList<Category> Categories { get; private set; } = new List<Category>();

    public static readonly string[] Neighborhoods = { "Town", "Boocame", "Iftin", "Wadajir", "Israac","Waaberi","Hodon","Halgan","1Da Augosto" };

    public enum AccountType
    {
        Client = 0,
        ServiceProvider = 1
    }

    public class InputModel
    {
        [Required, StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), StringLength(100, MinimumLength = 8)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "The passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required]
        public AccountType AccountType { get; set; } = AccountType.Client;

        [Required, StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string Neighborhood { get; set; } = string.Empty;

        // Service Provider-only fields — validated conditionally in OnPostAsync
        // rather than with attributes, since they don't apply to clients.
        public int? CategoryId { get; set; }

        public string? Headline { get; set; }

        public string? Bio { get; set; }

        public int? YearsOfExperience { get; set; }

        public decimal? HourlyRate { get; set; }
    }

    public async Task OnGetAsync()
    {
        Categories = await _categoryService.GetActiveAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Categories = await _categoryService.GetActiveAsync();

        if (Input.AccountType == AccountType.ServiceProvider)
        {
            if (Input.CategoryId is null or 0)
            {
                ModelState.AddModelError("Input.CategoryId", "Choose the trade category that best fits your work.");
            }

            if (string.IsNullOrWhiteSpace(Input.Headline))
            {
                ModelState.AddModelError("Input.Headline", "A short professional headline is required.");
            }
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = new ApplicationUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            FirstName = Input.FirstName.Trim(),
            LastName = Input.LastName.Trim(),
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, Input.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        if (Input.AccountType == AccountType.ServiceProvider)
        {
            await _userManager.AddToRoleAsync(user, "ServiceProvider");
            await _serviceProviderService.CreateProfileAsync(user.Id, new ServiceProviderProfileEditInput
            {
                Headline = Input.Headline ?? string.Empty,
                Bio = Input.Bio ?? string.Empty,
                CategoryId = Input.CategoryId ?? 0,
                YearsOfExperience = Input.YearsOfExperience ?? 0,
                HourlyRate = Input.HourlyRate,
                Address = Input.Neighborhood,
                PhoneNumber = Input.PhoneNumber,
                AvailabilityStatus = Domain.Enums.AvailabilityStatus.Available,
                SkillIds = new List<int>()
            });
        }
        else
        {
            await _userManager.AddToRoleAsync(user, "Client");
            await _clientProfileService.CreateProfileAsync(user.Id, Input.Neighborhood, Input.PhoneNumber);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);

        return Input.AccountType == AccountType.ServiceProvider
            ? RedirectToPage("/Dashboard", new { area = "ServiceProvider" })
            : RedirectToPage("/Dashboard", new { area = "Client" });
    }
}
