using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Domain.Enums;
using UnifiedFreelanceArtisansDirectory.Services;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Pages;

public class BrowseModel : PageModel
{
    private readonly IServiceProviderService _serviceProviderService;
    private readonly ICategoryService _categoryService;

    public BrowseModel(IServiceProviderService serviceProviderService, ICategoryService categoryService)
    {
        _serviceProviderService = serviceProviderService;
        _categoryService = categoryService;
    }

    [BindProperty(SupportsGet = true)]
    public string? Keyword { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? CategoryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Address { get; set; }

    [BindProperty(SupportsGet = true)]
    public double? MinimumRating { get; set; }

    [BindProperty(SupportsGet = true)]
    public AvailabilityStatus? AvailabilityStatus { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public PagedResult<ServiceProviderSummaryViewModel> Results { get; private set; } = new();

    public IReadOnlyList<Category> Categories { get; private set; } = new List<Category>();

    public static readonly string[] Addresses = { "Garowe Town Center", "Boocame", "Iftin", "Wadajir", "Israac" };

    public async Task OnGetAsync()
    {
        Categories = await _categoryService.GetActiveAsync();
        Results = await _serviceProviderService.SearchAsync(Keyword, CategoryId, Address, MinimumRating, AvailabilityStatus, PageNumber, 9);
    }
}
