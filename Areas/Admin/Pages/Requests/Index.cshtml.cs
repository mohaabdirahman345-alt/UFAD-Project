using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Services;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Areas.Admin.Pages.Requests;

public class IndexModel : PageModel
{
    private readonly IServiceRequestService _serviceRequestService;

    public IndexModel(IServiceRequestService serviceRequestService)
    {
        _serviceRequestService = serviceRequestService;
    }

    public IReadOnlyList<ServiceRequestSummaryViewModel> Requests { get; private set; } = new List<ServiceRequestSummaryViewModel>();

    public async Task OnGetAsync()
    {
        Requests = await _serviceRequestService.GetAllForAdminAsync();
    }
}
