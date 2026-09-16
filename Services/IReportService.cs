using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Domain.Enums;

namespace UnifiedFreelanceArtisansDirectory.Services;

public interface IReportService
{
    Task SubmitAsync(string reporterUserId, string reportedUserId, int? reportedReviewId, ReportReason reason, string details);

    Task<IReadOnlyList<Report>> GetByStatusAsync(ReportStatus status);

    Task ResolveAsync(int reportId, string resolvedByUserId, string resolutionNotes);

    Task DismissAsync(int reportId, string resolvedByUserId, string resolutionNotes);
}
