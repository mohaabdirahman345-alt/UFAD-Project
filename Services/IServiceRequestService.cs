using UnifiedFreelanceArtisansDirectory.Services.ViewModels;

namespace UnifiedFreelanceArtisansDirectory.Services;

public interface IServiceRequestService
{
    Task<ServiceRequestDetailViewModel> CreateRequestAsync(int clientProfileId, string createdByUserId, ServiceRequestCreateInput input);

    Task<ServiceRequestDetailViewModel?> GetDetailsAsync(int serviceRequestId);

    Task<IReadOnlyList<ServiceRequestSummaryViewModel>> GetForClientAsync(int clientProfileId);

    Task<IReadOnlyList<ServiceRequestSummaryViewModel>> GetForServiceProviderAsync(int serviceProviderProfileId);

    /// <summary>Full platform-wide listing for the admin oversight page.</summary>
    Task<IReadOnlyList<ServiceRequestSummaryViewModel>> GetAllForAdminAsync();

    Task AcceptAsync(int serviceRequestId, int serviceProviderProfileId, string acceptedByUserId);

    Task DeclineAsync(int serviceRequestId, int serviceProviderProfileId, string declinedByUserId, string reason);

    Task CompleteAsync(int serviceRequestId, int serviceProviderProfileId, string completedByUserId);

    Task CancelAsync(int serviceRequestId, int clientProfileId, string cancelledByUserId, string reason);
}
