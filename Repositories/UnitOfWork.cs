using UnifiedFreelanceArtisansDirectory.Data;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;

namespace UnifiedFreelanceArtisansDirectory.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    private IServiceProviderRepository? _serviceProviders;
    private IGenericRepository<ClientProfile>? _clients;
    private IGenericRepository<Category>? _categories;
    private IGenericRepository<Skill>? _skills;
    private IGenericRepository<ServiceProviderSkill>? _serviceProviderSkills;
    private IGenericRepository<PortfolioItem>? _portfolioItems;
    private IGenericRepository<Review>? _reviews;
    private IGenericRepository<Favorite>? _favorites;
    private IGenericRepository<Report>? _reports;
    private IGenericRepository<ServiceRequest>? _serviceRequests;
    private IGenericRepository<ServiceRequestStatusHistory>? _serviceRequestStatusHistories;
    private IGenericRepository<ServiceProviderApprovalHistory>? _serviceProviderApprovalHistories;
    private IGenericRepository<Notification>? _notifications;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IServiceProviderRepository ServiceProviders => _serviceProviders ??= new ServiceProviderRepository(_context);

    public IGenericRepository<ClientProfile> Clients => _clients ??= new GenericRepository<ClientProfile>(_context);

    public IGenericRepository<Category> Categories => _categories ??= new GenericRepository<Category>(_context);

    public IGenericRepository<Skill> Skills => _skills ??= new GenericRepository<Skill>(_context);

    public IGenericRepository<ServiceProviderSkill> ServiceProviderSkills =>
        _serviceProviderSkills ??= new GenericRepository<ServiceProviderSkill>(_context);

    public IGenericRepository<PortfolioItem> PortfolioItems =>
        _portfolioItems ??= new GenericRepository<PortfolioItem>(_context);

    public IGenericRepository<Review> Reviews => _reviews ??= new GenericRepository<Review>(_context);

    public IGenericRepository<Favorite> Favorites => _favorites ??= new GenericRepository<Favorite>(_context);

    public IGenericRepository<Report> Reports => _reports ??= new GenericRepository<Report>(_context);

    public IGenericRepository<ServiceRequest> ServiceRequests =>
        _serviceRequests ??= new GenericRepository<ServiceRequest>(_context);

    public IGenericRepository<ServiceRequestStatusHistory> ServiceRequestStatusHistories =>
        _serviceRequestStatusHistories ??= new GenericRepository<ServiceRequestStatusHistory>(_context);

    public IGenericRepository<ServiceProviderApprovalHistory> ServiceProviderApprovalHistories =>
        _serviceProviderApprovalHistories ??= new GenericRepository<ServiceProviderApprovalHistory>(_context);

    public IGenericRepository<Notification> Notifications =>
        _notifications ??= new GenericRepository<Notification>(_context);

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
