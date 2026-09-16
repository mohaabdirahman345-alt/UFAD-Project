using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Domain.Enums;
using UnifiedFreelanceArtisansDirectory.Services;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Areas.Admin.Pages;

public class DashboardModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IReportService _reportService;
    private readonly IServiceProviderService _serviceProviderService;
    private readonly IServiceRequestService _serviceRequestService;

    public DashboardModel(
        UserManager<ApplicationUser> userManager,
        IReportService reportService,
        IServiceProviderService serviceProviderService,
        IServiceRequestService serviceRequestService)
    {
        _userManager = userManager;
        _reportService = reportService;
        _serviceProviderService = serviceProviderService;
        _serviceRequestService = serviceRequestService;
    }

    public int ServiceProviderCount { get; private set; }

    public int ClientCount { get; private set; }

    public int PendingReportCount { get; private set; }

    public int PendingApprovalCount { get; private set; }

    /// <summary>Last six months, oldest first: (label, count).</summary>
    public List<(string Label, int Count)> MonthlyRequests { get; private set; } = new();

    public List<(string Category, int Count)> TopCategories { get; private set; } = new();

    public List<(string Status, int Count)> StatusDistribution { get; private set; } = new();

    public IReadOnlyList<ServiceRequestSummaryViewModel> RecentRequests { get; private set; } = new List<ServiceRequestSummaryViewModel>();

    public async Task OnGetAsync()
    {
        ServiceProviderCount = (await _userManager.GetUsersInRoleAsync("ServiceProvider")).Count;
        ClientCount = (await _userManager.GetUsersInRoleAsync("Client")).Count;
        PendingReportCount = (await _reportService.GetByStatusAsync(ReportStatus.Pending)).Count;

        var allProviders = await _serviceProviderService.GetAllForAdminAsync();
        PendingApprovalCount = allProviders.Count(p => p.ApprovalStatus == ApprovalStatus.PendingApproval);

        var allRequests = await _serviceRequestService.GetAllForAdminAsync();

        var today = DateTime.UtcNow.Date;
        var months = Enumerable.Range(0, 6)
            .Select(offset => new DateTime(today.Year, today.Month, 1).AddMonths(-offset))
            .Reverse()
            .ToList();

        MonthlyRequests = months
            .Select(m => (
                Label: m.ToString("MMM"),
                Count: allRequests.Count(r => r.CreatedOn.Year == m.Year && r.CreatedOn.Month == m.Month)))
            .ToList();

        TopCategories = allRequests
            .Where(r => !string.IsNullOrEmpty(r.CategoryName))
            .GroupBy(r => r.CategoryName)
            .Select(g => (Category: g.Key, Count: g.Count()))
            .OrderByDescending(g => g.Count)
            .Take(6)
            .ToList();

        StatusDistribution = allRequests
            .GroupBy(r => r.Status)
            .Select(g => (Status: g.Key.ToString(), Count: g.Count()))
            .OrderByDescending(g => g.Count)
            .ToList();

        RecentRequests = allRequests.Take(6).ToList();
    }
}
