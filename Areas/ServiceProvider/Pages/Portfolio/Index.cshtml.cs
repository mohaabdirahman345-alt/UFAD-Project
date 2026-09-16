using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Services;

namespace UnifiedFreelanceArtisansDirectory.Areas.ServiceProvider.Pages.Portfolio;

public class IndexModel : PageModel
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    private readonly IServiceProviderService _serviceProviderService;
    private readonly IPortfolioService _portfolioService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;

    public IndexModel(
        IServiceProviderService serviceProviderService,
        IPortfolioService portfolioService,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment environment)
    {
        _serviceProviderService = serviceProviderService;
        _portfolioService = portfolioService;
        _userManager = userManager;
        _environment = environment;
    }

    public ServiceProviderProfile Profile { get; private set; } = null!;

    [BindProperty]
    public UploadInput NewItem { get; set; } = new();

    public class UploadInput
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime CompletedOn { get; set; } = DateTime.UtcNow.Date;

        public IFormFile? Image { get; set; }
    }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var profile = await _serviceProviderService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (profile is null)
        {
            return NotFound();
        }

        Profile = profile;
        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        var profile = await _serviceProviderService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (profile is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(NewItem.Title))
        {
            ModelState.AddModelError("NewItem.Title", "A title is required.");
            Profile = profile;
            return Page();
        }

        var imagePath = "/images/portfolio/placeholder.jpg";

        if (NewItem.Image is { Length: > 0 })
        {
            var extension = Path.GetExtension(NewItem.Image.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("NewItem.Image", "Only JPG, PNG, or WEBP images are allowed.");
                Profile = profile;
                return Page();
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var folderPath = Path.Combine(_environment.WebRootPath, "images", "portfolio");
            Directory.CreateDirectory(folderPath);

            var fullPath = Path.Combine(folderPath, fileName);
            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await NewItem.Image.CopyToAsync(stream);
            }

            imagePath = $"/images/portfolio/{fileName}";
        }

        await _portfolioService.AddAsync(profile.Id, NewItem.Title, NewItem.Description, imagePath, NewItem.CompletedOn);

        StatusMessage = "Portfolio item added.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int portfolioItemId)
    {
        var profile = await _serviceProviderService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (profile is null)
        {
            return NotFound();
        }

        await _portfolioService.DeleteAsync(portfolioItemId, profile.Id);
        StatusMessage = "Portfolio item removed.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditAsync(int portfolioItemId, string editTitle, string editDescription, DateTime editCompletedOn)
    {
        var profile = await _serviceProviderService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (profile is null)
        {
            return NotFound();
        }

        await _portfolioService.UpdateAsync(portfolioItemId, profile.Id, editTitle, editDescription, editCompletedOn);
        StatusMessage = "Portfolio item updated.";
        return RedirectToPage();
    }
}
