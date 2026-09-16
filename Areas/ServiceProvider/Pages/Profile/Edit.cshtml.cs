using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Domain.Enums;
using UnifiedFreelanceArtisansDirectory.Services;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Areas.ServiceProvider.Pages.Profile;

public class EditModel : PageModel
{
    private readonly IServiceProviderService _serviceProviderService;
    private readonly ICategoryService _categoryService;
    private readonly ISkillService _skillService;
    private readonly UserManager<ApplicationUser> _userManager;

    public EditModel(
        IServiceProviderService serviceProviderService,
        ICategoryService categoryService,
        ISkillService skillService,
        UserManager<ApplicationUser> userManager)
    {
        _serviceProviderService = serviceProviderService;
        _categoryService = categoryService;
        _skillService = skillService;
        _userManager = userManager;
    }

    [BindProperty]
    public ServiceProviderProfileEditInput Input { get; set; } = new();

    public ApprovalStatus ApprovalStatus { get; private set; }

    public IReadOnlyList<Category> Categories { get; private set; } = new List<Category>();

    public IReadOnlyList<Skill> AllSkills { get; private set; } = new List<Skill>();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var profile = await _serviceProviderService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (profile is null)
        {
            return NotFound();
        }

        ApprovalStatus = profile.ApprovalStatus;
        Categories = await _categoryService.GetActiveAsync();
        AllSkills = await _skillService.GetAllAsync();

        Input = new ServiceProviderProfileEditInput
        {
            Headline = profile.Headline,
            Bio = profile.Bio,
            CategoryId = profile.CategoryId,
            YearsOfExperience = profile.YearsOfExperience,
            HourlyRate = profile.HourlyRate,
            Address = profile.Address,
            PhoneNumber = profile.PhoneNumber,
            AvailabilityStatus = profile.AvailabilityStatus,
            SkillIds = profile.ServiceProviderSkills.Select(fs => fs.SkillId).ToList()
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var userId = _userManager.GetUserId(User)!;
        var profile = await _serviceProviderService.GetByUserIdAsync(userId);
        if (profile is null)
        {
            return NotFound();
        }

        ApprovalStatus = profile.ApprovalStatus;
        Categories = await _categoryService.GetActiveAsync();
        AllSkills = await _skillService.GetAllAsync();

        if (string.IsNullOrWhiteSpace(Input.Headline))
        {
            ModelState.AddModelError("Input.Headline", "A professional headline is required.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _serviceProviderService.UpdateProfileAsync(profile.Id, Input);

        if ((profile.ApprovalStatus == ApprovalStatus.Draft || profile.ApprovalStatus == ApprovalStatus.Rejected)
            && IsReadyForApproval(Input))
        {
            await _serviceProviderService.SubmitForApprovalAsync(profile.Id, userId);
            StatusMessage = "Your profile is complete and has been submitted for administrator review.";
        }
        else
        {
            StatusMessage = "Your profile has been updated.";
        }

        return RedirectToPage();
    }

    private static bool IsReadyForApproval(ServiceProviderProfileEditInput input)
    {
        return !string.IsNullOrWhiteSpace(input.Headline)
            && !string.IsNullOrWhiteSpace(input.Bio)
            && input.CategoryId > 0
            && input.YearsOfExperience > 0
            && input.HourlyRate.HasValue
            && !string.IsNullOrWhiteSpace(input.Address)
            && !string.IsNullOrWhiteSpace(input.PhoneNumber)
            && input.SkillIds.Any();
    }

    public async Task<IActionResult> OnPostSubmitForApprovalAsync()
    {
        var profile = await _serviceProviderService.GetByUserIdAsync(_userManager.GetUserId(User)!);
        if (profile is null)
        {
            return NotFound();
        }

        await _serviceProviderService.SubmitForApprovalAsync(profile.Id, _userManager.GetUserId(User)!);
        StatusMessage = "Your profile has been submitted for administrator review.";
        return RedirectToPage();
    }
}
