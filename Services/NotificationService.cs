using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Domain.Enums;
using UnifiedFreelanceArtisansDirectory.Repositories;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Services;

public class NotificationService : INotificationService
{
    private const string AdministratorRole = "Administrator";

    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task NotifyUserAsync(
        string userId,
        NotificationType type,
        string title,
        string message,
        string? linkUrl = null,
        string? relatedEntityType = null,
        int? relatedEntityId = null)
    {
        await _unitOfWork.Notifications.AddAsync(new Notification
        {
            UserId = userId,
            Type = type,
            Title = title.Trim(),
            Message = message.Trim(),
            LinkUrl = linkUrl,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId
        });
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task NotifyAdminsAsync(
        NotificationType type,
        string title,
        string message,
        string? linkUrl = null,
        string? relatedEntityType = null,
        int? relatedEntityId = null)
    {
        var admins = await _userManager.GetUsersInRoleAsync(AdministratorRole);
        if (admins.Count == 0)
        {
            return;
        }

        foreach (var admin in admins)
        {
            await _unitOfWork.Notifications.AddAsync(new Notification
            {
                UserId = admin.Id,
                Type = type,
                Title = title.Trim(),
                Message = message.Trim(),
                LinkUrl = linkUrl,
                RelatedEntityType = relatedEntityType,
                RelatedEntityId = relatedEntityId
            });
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<NotificationViewModel>> GetForUserAsync(string userId, int take = 50)
    {
        take = take is < 1 or > 100 ? 50 : take;

        return await _unitOfWork.Notifications.Query()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedOn)
            .Take(take)
            .Select(n => new NotificationViewModel
            {
                Id = n.Id,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                LinkUrl = n.LinkUrl,
                IsRead = n.IsRead,
                CreatedOn = n.CreatedOn
            })
            .ToListAsync();
    }

    public Task<int> GetUnreadCountAsync(string userId) =>
        _unitOfWork.Notifications.Query()
            .CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task MarkAllReadAsync(string userId)
    {
        var unread = await _unitOfWork.Notifications.Query()
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        if (unread.Count == 0)
        {
            return;
        }

        foreach (var notification in unread)
        {
            notification.IsRead = true;
            _unitOfWork.Notifications.Update(notification);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MarkReadAsync(int notificationId, string userId)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
        if (notification is null || notification.UserId != userId || notification.IsRead)
        {
            return;
        }

        notification.IsRead = true;
        _unitOfWork.Notifications.Update(notification);
        await _unitOfWork.SaveChangesAsync();
    }
}
