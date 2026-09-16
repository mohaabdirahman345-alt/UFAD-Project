using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Services;

namespace UnifiedFreelanceArtisansDirectory.Areas.Admin.Pages.Skills;

public class IndexModel : PageModel
{
    private readonly ISkillService _skillService;
    private readonly ICategoryService _categoryService;

    public IndexModel(ISkillService skillService, ICategoryService categoryService)
    {
        _skillService = skillService;
        _categoryService = categoryService;
    }

    public IReadOnlyList<Skill> Skills { get; private set; } = new List<Skill>();

    public IReadOnlyList<Category> Categories { get; private set; } = new List<Category>();

    [BindProperty]
    public string NewSkillName { get; set; } = string.Empty;

    [BindProperty]
    public int NewSkillCategoryId { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        Skills = await _skillService.GetAllAsync();
        Categories = await _categoryService.GetActiveAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (string.IsNullOrWhiteSpace(NewSkillName))
        {
            ModelState.AddModelError(nameof(NewSkillName), "A skill name is required.");
            Skills = await _skillService.GetAllAsync();
            Categories = await _categoryService.GetActiveAsync();
            return Page();
        }

        await _skillService.CreateAsync(NewSkillName, NewSkillCategoryId);
        StatusMessage = $"Skill \"{NewSkillName}\" added.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int skillId)
    {
        await _skillService.DeleteAsync(skillId);
        StatusMessage = "Skill removed.";
        return RedirectToPage();
    }
}
