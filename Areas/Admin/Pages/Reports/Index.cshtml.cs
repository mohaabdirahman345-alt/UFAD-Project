using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Domain.Enums;
using UnifiedFreelanceArtisansDirectory.Services;

namespace UnifiedFreelanceArtisansDirectory.Areas.Admin.Pages.Reports;

public class IndexModel : PageModel
{
    private readonly IReportService _reportService;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(IReportService reportService, UserManager<ApplicationUser> userManager)
    {
        _reportService = reportService;
        _userManager = userManager;
    }

    public IReadOnlyList<Report> PendingReports { get; private set; } = new List<Report>();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        PendingReports = await _reportService.GetByStatusAsync(ReportStatus.Pending);
    }

    public async Task<IActionResult> OnPostResolveAsync(int reportId, string notes)
    {
        await _reportService.ResolveAsync(reportId, _userManager.GetUserId(User)!, notes);
        StatusMessage = "Report marked resolved.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDismissAsync(int reportId, string notes)
    {
        await _reportService.DismissAsync(reportId, _userManager.GetUserId(User)!, notes);
        StatusMessage = "Report dismissed.";
        return RedirectToPage();
    }
}
