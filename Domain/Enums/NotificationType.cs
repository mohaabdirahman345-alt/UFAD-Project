namespace UnifiedFreelanceArtisansDirectory.Domain.Enums;

public enum NotificationType
{
    ProfilePendingApproval = 1,
    ProfileApproved = 2,
    ProfileRejected = 3,
    ServiceRequestCreated = 4,
    ServiceRequestAccepted = 5,
    ServiceRequestDeclined = 6,
    ServiceRequestCompleted = 7,
    ServiceRequestCancelled = 8,
    ReviewReceived = 9,
    ReportSubmitted = 10
}
