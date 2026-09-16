using UnifiedFreelanceArtisansDirectory.Domain.Entities;

namespace UnifiedFreelanceArtisansDirectory.Repositories;

/// <summary>
/// Aggregates every repository behind a single scope so a service method
/// that touches several tables (e.g. adding a review and recalculating a
/// service provider's rating) commits as one transaction via SaveChangesAsync.
/// </summary>
public interface IUnitOfWork
{
    IServiceProviderRepository ServiceProviders { get; }

    IGenericRepository<ClientProfile> Clients { get; }

    IGenericRepository<Category> Categories { get; }

    IGenericRepository<Skill> Skills { get; }

    IGenericRepository<ServiceProviderSkill> ServiceProviderSkills { get; }

    IGenericRepository<PortfolioItem> PortfolioItems { get; }

    IGenericRepository<Review> Reviews { get; }

    IGenericRepository<Favorite> Favorites { get; }

    IGenericRepository<Report> Reports { get; }

    IGenericRepository<ServiceRequest> ServiceRequests { get; }

    IGenericRepository<ServiceRequestStatusHistory> ServiceRequestStatusHistories { get; }

    IGenericRepository<ServiceProviderApprovalHistory> ServiceProviderApprovalHistories { get; }

    IGenericRepository<Notification> Notifications { get; }

    Task<int> SaveChangesAsync();
}
