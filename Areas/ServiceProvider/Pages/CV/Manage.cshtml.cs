using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Services;

namespace UnifiedFreelanceArtisansDirectory.Areas.ServiceProvider.Pages.CV;

public class ManageModel : PageModel
{
    private readonly IServiceProviderService _serviceProviderService;
    private readonly IServiceProviderCVService _cvService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ManageModel(
        IServiceProviderService serviceProviderService,
        IServiceProviderCVService cvService,
        UserManager<ApplicationUser> userManager)
    {
        _serviceProviderService = serviceProviderService;
        _cvService = cvService;
        _userManager = userManager;
    }

    public ServiceProviderCV? CurrentCV { get; private set; }

    public ServiceProviderProfile? Profile { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userId = _userManager.GetUserId(User)!;
        Profile = await _serviceProviderService.GetByUserIdAsync(userId);
        
        if (Profile is null)
        {
            return NotFound();
        }

        CurrentCV = await _cvService.GetByServiceProviderProfileIdAsync(Profile.Id);

        return Page();
    }

    public async Task<IActionResult> OnPostUploadAsync(IFormFile cvFile)
    {
        var userId = _userManager.GetUserId(User)!;
        Profile = await _serviceProviderService.GetByUserIdAsync(userId);

        if (Profile is null)
        {
            return NotFound();
        }

        try
        {
            _cvService.ValidateCVFile(cvFile);
            await _cvService.UploadCVAsync(Profile.Id, cvFile, "/uploads/cvs");
            StatusMessage = "CV uploaded successfully.";
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            ErrorMessage = "An error occurred while uploading the CV. Please try again.";
            Console.WriteLine($"CV upload error: {ex.Message}");
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var userId = _userManager.GetUserId(User)!;
        Profile = await _serviceProviderService.GetByUserIdAsync(userId);

        if (Profile is null)
        {
            return NotFound();
        }

        try
        {
            await _cvService.DeleteCVAsync(Profile.Id);
            StatusMessage = "CV deleted successfully.";
        }
        catch (Exception ex)
        {
            ErrorMessage = "An error occurred while deleting the CV. Please try again.";
            Console.WriteLine($"CV delete error: {ex.Message}");
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDownloadAsync()
    {
        var userId = _userManager.GetUserId(User)!;
        Profile = await _serviceProviderService.GetByUserIdAsync(userId);

        if (Profile is null)
        {
            return NotFound();
        }

        CurrentCV = await _cvService.GetByServiceProviderProfileIdAsync(Profile.Id);

        if (CurrentCV is null)
        {
            return NotFound("CV not found.");
        }

        var filePath = await _cvService.GetCVFilePathAsync(CurrentCV.Id);
        if (filePath is null || !System.IO.File.Exists(filePath))
        {
            return NotFound("CV file not found on server.");
        }

        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        return File(fileBytes, CurrentCV.ContentType, CurrentCV.OriginalFileName);
    }
}
