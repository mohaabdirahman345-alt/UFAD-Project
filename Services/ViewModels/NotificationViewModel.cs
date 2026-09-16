using UnifiedFreelanceArtisansDirectory.Domain.Enums;

namespace UnifiedFreelanceArtisansDirectory.Services.ViewModels;

public class NotificationViewModel
{
    public int Id { get; set; }

    public NotificationType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public string? LinkUrl { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedOn { get; set; }
}
