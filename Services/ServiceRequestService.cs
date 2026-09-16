using Microsoft.EntityFrameworkCore;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Domain.Enums;
using UnifiedFreelanceArtisansDirectory.Repositories;
using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Services;

public class ServiceRequestService : IServiceRequestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public ServiceRequestService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<ServiceRequestDetailViewModel> CreateRequestAsync(int clientProfileId, string createdByUserId, ServiceRequestCreateInput input)
    {
        var request = new ServiceRequest
        {
            ClientProfileId = clientProfileId,
            ServiceProviderProfileId = input.ServiceProviderProfileId,
            Title = input.Title.Trim(),
            Description = input.Description.Trim(),
            PreferredDate = input.PreferredDate,
            Status = ServiceRequestStatus.Pending
        };

        await _unitOfWork.ServiceRequests.AddAsync(request);
        await _unitOfWork.SaveChangesAsync();

        await AddHistoryAsync(request.Id, null, ServiceRequestStatus.Pending, createdByUserId, "Request submitted.");
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.ServiceRequests.Query()
            .Include(r => r.ClientProfile).ThenInclude(c => c.User)
            .Include(r => r.ServiceProviderProfile)
            .FirstAsync(r => r.Id == request.Id);

        var clientName = created.ClientProfile.User.FullName;
        var title = created.Title;

        await _notificationService.NotifyUserAsync(
            created.ServiceProviderProfile.UserId,
            NotificationType.ServiceRequestCreated,
            "New service request",
            $"{clientName} sent you a request: \"{title}\".",
            "/ServiceProvider/Requests",
            "ServiceRequest",
            created.Id);

        await _notificationService.NotifyAdminsAsync(
            NotificationType.ServiceRequestCreated,
            "New service request",
            $"{clientName} submitted \"{title}\".",
            "/Admin/Requests",
            "ServiceRequest",
            created.Id);

        return (await GetDetailsAsync(request.Id))!;
    }

    public async Task<ServiceRequestDetailViewModel?> GetDetailsAsync(int serviceRequestId)
    {
        var request = await _unitOfWork.ServiceRequests.Query()
            .Include(r => r.ClientProfile).ThenInclude(c => c.User)
            .Include(r => r.ServiceProviderProfile).ThenInclude(f => f.User)
            .Include(r => r.StatusHistory).ThenInclude(h => h.ChangedByUser)
            .FirstOrDefaultAsync(r => r.Id == serviceRequestId);

        return request is null ? null : MapToDetail(request);
    }

    public async Task<IReadOnlyList<ServiceRequestSummaryViewModel>> GetForClientAsync(int clientProfileId) =>
        (await _unitOfWork.ServiceRequests.Query()
            .Include(r => r.ClientProfile).ThenInclude(c => c.User)
            .Include(r => r.ServiceProviderProfile).ThenInclude(f => f.User)
            .Where(r => r.ClientProfileId == clientProfileId)
            .OrderByDescending(r => r.CreatedOn)
            .ToListAsync())
        .Select(MapToSummary)
        .ToList();

    public async Task<IReadOnlyList<ServiceRequestSummaryViewModel>> GetForServiceProviderAsync(int serviceProviderProfileId) =>
        (await _unitOfWork.ServiceRequests.Query()
            .Include(r => r.ClientProfile).ThenInclude(c => c.User)
            .Include(r => r.ServiceProviderProfile).ThenInclude(f => f.User)
            .Where(r => r.ServiceProviderProfileId == serviceProviderProfileId)
            // Pending requests need attention first; everything else newest first.
            .OrderBy(r => r.Status == ServiceRequestStatus.Pending ? 0 : 1)
            .ThenByDescending(r => r.CreatedOn)
            .ToListAsync())
        .Select(MapToSummary)
        .ToList();

    public async Task<IReadOnlyList<ServiceRequestSummaryViewModel>> GetAllForAdminAsync() =>
        (await _unitOfWork.ServiceRequests.Query()
            .Include(r => r.ClientProfile).ThenInclude(c => c.User)
            .Include(r => r.ServiceProviderProfile).ThenInclude(f => f.User)
            .Include(r => r.ServiceProviderProfile).ThenInclude(f => f.Category)
            .OrderByDescending(r => r.CreatedOn)
            .ToListAsync())
        .Select(MapToSummary)
        .ToList();

    public async Task AcceptAsync(int serviceRequestId, int serviceProviderProfileId, string acceptedByUserId)
    {
        var request = await GetOwnedByProviderOrThrowAsync(serviceRequestId, serviceProviderProfileId);
        EnsureStatus(request, ServiceRequestStatus.Pending);

        var previousStatus = request.Status;
        request.Status = ServiceRequestStatus.Accepted;
        request.RespondedOn = DateTime.UtcNow;

        _unitOfWork.ServiceRequests.Update(request);
        await AddHistoryAsync(request.Id, previousStatus, ServiceRequestStatus.Accepted, acceptedByUserId, null);
        await _unitOfWork.SaveChangesAsync();

        await NotifyClientOfStatusAsync(request.Id, NotificationType.ServiceRequestAccepted, "Request accepted",
            title => $"Your request \"{title}\" was accepted.");
    }

    public async Task DeclineAsync(int serviceRequestId, int serviceProviderProfileId, string declinedByUserId, string reason)
    {
        var request = await GetOwnedByProviderOrThrowAsync(serviceRequestId, serviceProviderProfileId);
        EnsureStatus(request, ServiceRequestStatus.Pending);

        var previousStatus = request.Status;
        request.Status = ServiceRequestStatus.Declined;
        request.RespondedOn = DateTime.UtcNow;
        request.DeclineReason = reason.Trim();

        _unitOfWork.ServiceRequests.Update(request);
        await AddHistoryAsync(request.Id, previousStatus, ServiceRequestStatus.Declined, declinedByUserId, reason);
        await _unitOfWork.SaveChangesAsync();

        await NotifyClientOfStatusAsync(request.Id, NotificationType.ServiceRequestDeclined, "Request declined",
            title => $"Your request \"{title}\" was declined.");
    }

    public async Task CompleteAsync(int serviceRequestId, int serviceProviderProfileId, string completedByUserId)
    {
        var request = await GetOwnedByProviderOrThrowAsync(serviceRequestId, serviceProviderProfileId);
        EnsureStatus(request, ServiceRequestStatus.Accepted);

        var previousStatus = request.Status;
        request.Status = ServiceRequestStatus.Completed;
        request.CompletedOn = DateTime.UtcNow;

        _unitOfWork.ServiceRequests.Update(request);
        await AddHistoryAsync(request.Id, previousStatus, ServiceRequestStatus.Completed, completedByUserId, null);
        await _unitOfWork.SaveChangesAsync();

        await NotifyClientOfStatusAsync(request.Id, NotificationType.ServiceRequestCompleted, "Request completed",
            title => $"Your request \"{title}\" was marked complete.");
    }

    public async Task CancelAsync(int serviceRequestId, int clientProfileId, string cancelledByUserId, string reason)
    {
        var request = await _unitOfWork.ServiceRequests.GetByIdAsync(serviceRequestId)
            ?? throw new InvalidOperationException($"Service request {serviceRequestId} was not found.");

        if (request.ClientProfileId != clientProfileId)
        {
            throw new UnauthorizedAccessException("You may only cancel your own requests.");
        }

        // Only a still-pending request can be withdrawn — once a provider
        // has accepted, cancelling would need their agreement too, which
        // this simple flow doesn't model.
        EnsureStatus(request, ServiceRequestStatus.Pending);

        var previousStatus = request.Status;
        request.Status = ServiceRequestStatus.Cancelled;
        request.CancellationReason = reason.Trim();

        _unitOfWork.ServiceRequests.Update(request);
        await AddHistoryAsync(request.Id, previousStatus, ServiceRequestStatus.Cancelled, cancelledByUserId, reason);
        await _unitOfWork.SaveChangesAsync();

        var loaded = await _unitOfWork.ServiceRequests.Query()
            .Include(r => r.ClientProfile).ThenInclude(c => c.User)
            .Include(r => r.ServiceProviderProfile)
            .FirstAsync(r => r.Id == request.Id);

        await _notificationService.NotifyUserAsync(
            loaded.ServiceProviderProfile.UserId,
            NotificationType.ServiceRequestCancelled,
            "Request cancelled",
            $"{loaded.ClientProfile.User.FullName} cancelled \"{loaded.Title}\".",
            "/ServiceProvider/Requests",
            "ServiceRequest",
            loaded.Id);
    }

    private async Task NotifyClientOfStatusAsync(
        int serviceRequestId,
        NotificationType type,
        string notificationTitle,
        Func<string, string> messageFactory)
    {
        var request = await _unitOfWork.ServiceRequests.Query()
            .Include(r => r.ClientProfile)
            .FirstAsync(r => r.Id == serviceRequestId);

        await _notificationService.NotifyUserAsync(
            request.ClientProfile.UserId,
            type,
            notificationTitle,
            messageFactory(request.Title),
            "/Client/Requests",
            "ServiceRequest",
            request.Id);
    }

    private async Task<ServiceRequest> GetOwnedByProviderOrThrowAsync(int serviceRequestId, int serviceProviderProfileId)
    {
        var request = await _unitOfWork.ServiceRequests.GetByIdAsync(serviceRequestId)
            ?? throw new InvalidOperationException($"Service request {serviceRequestId} was not found.");

        if (request.ServiceProviderProfileId != serviceProviderProfileId)
        {
            throw new UnauthorizedAccessException("You may only act on requests sent to you.");
        }

        return request;
    }

    private static void EnsureStatus(ServiceRequest request, ServiceRequestStatus expected)
    {
        if (request.Status != expected)
        {
            throw new InvalidOperationException($"This request is {request.Status} and can't be moved from {expected}.");
        }
    }

    private async Task AddHistoryAsync(int serviceRequestId, ServiceRequestStatus? fromStatus, ServiceRequestStatus toStatus, string changedByUserId, string? notes)
    {
        await _unitOfWork.ServiceRequestStatusHistories.AddAsync(new ServiceRequestStatusHistory
        {
            ServiceRequestId = serviceRequestId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ChangedByUserId = changedByUserId,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        });
    }

    private static ServiceRequestSummaryViewModel MapToSummary(ServiceRequest r) => new()
    {
        Id = r.Id,
        ClientName = r.ClientProfile.User.FullName,
        ServiceProviderProfileId = r.ServiceProviderProfileId,
        ServiceProviderName = r.ServiceProviderProfile.User.FullName,
        CategoryName = r.ServiceProviderProfile.Category?.Name ?? string.Empty,
        Title = r.Title,
        PreferredDate = r.PreferredDate,
        Status = r.Status,
        CreatedOn = r.CreatedOn
    };

    private static ServiceRequestDetailViewModel MapToDetail(ServiceRequest r) => new()
    {
        Id = r.Id,
        ClientProfileId = r.ClientProfileId,
        ClientName = r.ClientProfile.User.FullName,
        ServiceProviderProfileId = r.ServiceProviderProfileId,
        ServiceProviderName = r.ServiceProviderProfile.User.FullName,
        Title = r.Title,
        Description = r.Description,
        PreferredDate = r.PreferredDate,
        Status = r.Status,
        CreatedOn = r.CreatedOn,
        RespondedOn = r.RespondedOn,
        CompletedOn = r.CompletedOn,
        DeclineReason = r.DeclineReason,
        CancellationReason = r.CancellationReason,
        StatusHistory = r.StatusHistory
            .OrderBy(h => h.ChangedOn)
            .Select(h => new StatusHistoryEntryViewModel
            {
                FromStatus = h.FromStatus?.ToString(),
                ToStatus = h.ToStatus.ToString(),
                ChangedOn = h.ChangedOn,
                ChangedByName = h.ChangedByUser.FullName,
                Notes = h.Notes
            })
            .ToList()
    };
}
