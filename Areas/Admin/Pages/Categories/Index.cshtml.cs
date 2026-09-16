using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Services;

namespace UnifiedFreelanceArtisansDirectory.Areas.Admin.Pages.Categories;

public class IndexModel : PageModel
{
    private readonly ICategoryService _categoryService;

    public IndexModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public IReadOnlyList<Category> Categories { get; private set; } = new List<Category>();

    [BindProperty]
    public CategoryInput NewCategory { get; set; } = new();

    public class CategoryInput
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string IconClass { get; set; } = string.Empty;
    }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        Categories = await _categoryService.GetAllAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCategory.Name))
        {
            ModelState.AddModelError("NewCategory.Name", "A category name is required.");
            Categories = await _categoryService.GetAllAsync();
            return Page();
        }

        await _categoryService.CreateAsync(NewCategory.Name, NewCategory.Description, NewCategory.IconClass);
        StatusMessage = $"Category \"{NewCategory.Name}\" created.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync(int categoryId, string name, string description, string iconClass)
    {
        await _categoryService.UpdateAsync(categoryId, name, description, iconClass);
        StatusMessage = "Category updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(int categoryId, bool isActive)
    {
        await _categoryService.SetActiveAsync(categoryId, !isActive);
        return RedirectToPage();
    }
}
