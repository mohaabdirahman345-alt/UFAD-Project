using System.Linq.Expressions;

namespace UnifiedFreelanceArtisansDirectory.Repositories;

/// <summary>
/// Baseline data access contract shared by every entity. Specific
/// repositories (e.g. IServiceProviderRepository) extend this with queries that
/// need eager loading or filtering too specific to belong here.
/// </summary>
public interface IGenericRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(object id);

    Task<IReadOnlyList<TEntity>> GetAllAsync();

    Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Exposes the underlying queryable for repositories that need to
    /// compose Include/OrderBy/paging beyond what FindAsync offers.
    /// </summary>
    IQueryable<TEntity> Query();

    Task AddAsync(TEntity entity);

    void Update(TEntity entity);

    void Remove(TEntity entity);
}
