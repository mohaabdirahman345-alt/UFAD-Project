using Microsoft.EntityFrameworkCore;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Domain.Enums;
using UnifiedFreelanceArtisansDirectory.Repositories;

namespace UnifiedFreelanceArtisansDirectory.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public ReportService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task SubmitAsync(string reporterUserId, string reportedUserId, int? reportedReviewId, ReportReason reason, string details)
    {
        var report = new Report
        {
            ReporterUserId = reporterUserId,
            ReportedUserId = reportedUserId,
            ReportedReviewId = reportedReviewId,
            Reason = reason,
            Details = details.Trim(),
            Status = ReportStatus.Pending
        };

        await _unitOfWork.Reports.AddAsync(report);
        await _unitOfWork.SaveChangesAsync();

        await _notificationService.NotifyAdminsAsync(
            NotificationType.ReportSubmitted,
            "New report",
            $"A user submitted a report ({reason}).",
            "/Admin/Reports",
            "Report",
            report.Id);
    }

    public async Task<IReadOnlyList<Report>> GetByStatusAsync(ReportStatus status) =>
        await _unitOfWork.Reports.Query()
            .Include(r => r.ReporterUser)
            .Include(r => r.ReportedUser)
            .Where(r => r.Status == status)
            .OrderBy(r => r.CreatedOn)
            .ToListAsync();

    public Task ResolveAsync(int reportId, string resolvedByUserId, string resolutionNotes) =>
        CloseAsync(reportId, resolvedByUserId, resolutionNotes, ReportStatus.Resolved);

    public Task DismissAsync(int reportId, string resolvedByUserId, string resolutionNotes) =>
        CloseAsync(reportId, resolvedByUserId, resolutionNotes, ReportStatus.Dismissed);

    private async Task CloseAsync(int reportId, string resolvedByUserId, string resolutionNotes, ReportStatus finalStatus)
    {
        var report = await _unitOfWork.Reports.GetByIdAsync(reportId)
            ?? throw new InvalidOperationException($"Report {reportId} was not found.");

        report.Status = finalStatus;
        report.ResolvedByUserId = resolvedByUserId;
        report.ResolutionNotes = resolutionNotes.Trim();
        report.ResolvedOn = DateTime.UtcNow;

        _unitOfWork.Reports.Update(report);
        await _unitOfWork.SaveChangesAsync();
    }
}
