using UnifiedFreelanceArtisansDirectory.Domain.Enums;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Services;

public interface INotificationService
{
    Task NotifyUserAsync(
        string userId,
        NotificationType type,
        string title,
        string message,
        string? linkUrl = null,
        string? relatedEntityType = null,
        int? relatedEntityId = null);

    Task NotifyAdminsAsync(
        NotificationType type,
        string title,
        string message,
        string? linkUrl = null,
        string? relatedEntityType = null,
        int? relatedEntityId = null);

    Task<IReadOnlyList<NotificationViewModel>> GetForUserAsync(string userId, int take = 50);

    Task<int> GetUnreadCountAsync(string userId);

    Task MarkAllReadAsync(string userId);

    Task MarkReadAsync(int notificationId, string userId);
}
