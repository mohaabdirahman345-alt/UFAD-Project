using Microsoft.EntityFrameworkCore;
using UnifiedFreelanceArtisansDirectory.Data;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Domain.Enums;

namespace UnifiedFreelanceArtisansDirectory.Repositories;

public class ServiceProviderRepository : GenericRepository<ServiceProviderProfile>, IServiceProviderRepository
{
    public ServiceProviderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<(IReadOnlyList<ServiceProviderProfile> Results, int TotalCount)> SearchAsync(
        string? keyword,
        int? categoryId,
        string? address,
        double? minimumRating,
        AvailabilityStatus? availabilityStatus,
        int pageNumber,
        int pageSize)
    {
        var query = DbSet
            .Include(f => f.User)
            .Include(f => f.Category)
            .Include(f => f.ServiceProviderSkills)
                .ThenInclude(fs => fs.Skill)
            // Only approved, active profiles are ever discoverable in the
            // public directory — this is the workflow's core gate.
            .Where(f => f.User.IsActive && f.ApprovalStatus == ApprovalStatus.Approved)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var trimmedKeyword = keyword.Trim();
            query = query.Where(f =>
                EF.Functions.Like(f.Headline, $"%{trimmedKeyword}%") ||
                EF.Functions.Like(f.Bio, $"%{trimmedKeyword}%") ||
                f.ServiceProviderSkills.Any(fs => EF.Functions.Like(fs.Skill.Name, $"%{trimmedKeyword}%")));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(f => f.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(address))
        {
            query = query.Where(f => f.Address == address);
        }

        if (minimumRating.HasValue)
        {
            query = query.Where(f => f.AverageRating >= minimumRating.Value);
        }

        if (availabilityStatus.HasValue)
        {
            query = query.Where(f => f.AvailabilityStatus == availabilityStatus.Value);
        }

        var totalCount = await query.CountAsync();

        var results = await query
            .OrderByDescending(f => f.IsVerified)
            .ThenByDescending(f => f.AverageRating)
            .ThenByDescending(f => f.ReviewCount)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (results, totalCount);
    }

    public async Task<ServiceProviderProfile?> GetProfileWithDetailsAsync(int serviceProviderProfileId) =>
        await DbSet
            .Include(f => f.User)
            .Include(f => f.Category)
            .Include(f => f.ServiceProviderSkills)
                .ThenInclude(fs => fs.Skill)
            .Include(f => f.PortfolioItems.OrderBy(p => p.DisplayOrder))
            .Include(f => f.Reviews.OrderByDescending(r => r.CreatedOn))
                .ThenInclude(r => r.ClientProfile)
                    .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(f => f.Id == serviceProviderProfileId);

    public async Task<ServiceProviderProfile?> GetProfileByUserIdAsync(string userId) =>
        await DbSet
            .Include(f => f.Category)
            .Include(f => f.ServiceProviderSkills)
                .ThenInclude(fs => fs.Skill)
            .Include(f => f.PortfolioItems.OrderBy(p => p.DisplayOrder))
            .FirstOrDefaultAsync(f => f.UserId == userId);

    public async Task<IReadOnlyList<ServiceProviderProfile>> GetTopRatedAsync(int count) =>
        await DbSet
            .Include(f => f.User)
            .Include(f => f.Category)
            .Where(f => f.User.IsActive && f.ApprovalStatus == ApprovalStatus.Approved && f.ReviewCount > 0)
            .OrderByDescending(f => f.IsVerified)
            .ThenByDescending(f => f.AverageRating)
            .ThenByDescending(f => f.ReviewCount)
            .Take(count)
            .ToListAsync();

    public async Task<IReadOnlyList<ServiceProviderProfile>> GetAllForAdminAsync() =>
        await DbSet
            .Include(f => f.User)
            .Include(f => f.Category)
            .Include(f => f.ServiceProviderSkills).ThenInclude(fs => fs.Skill)
            .OrderBy(f => f.ApprovalStatus == ApprovalStatus.PendingApproval ? 0 : 1)
            .ThenByDescending(f => f.CreatedOn)
            .ToListAsync();
}
