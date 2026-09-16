using UnifiedFreelanceArtisansDirectory.Domain.Enums;

namespace UnifiedFreelanceArtisansDirectory.Domain.Entities;

/// <summary>
/// A persisted in-app notification for a specific user. Created by domain
/// services when approval, request, review, or report events occur.
/// </summary>
public class Notification
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    public NotificationType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    /// <summary>Optional deep link within the app (e.g. /Admin/Approvals).</summary>
    public string? LinkUrl { get; set; }

    public string? RelatedEntityType { get; set; }

    public int? RelatedEntityId { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}
