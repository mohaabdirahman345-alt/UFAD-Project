using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UnifiedFreelanceArtisansDirectory.Data;

namespace UnifiedFreelanceArtisansDirectory.Repositories;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    protected readonly ApplicationDbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(object id) => await DbSet.FindAsync(id);

    public async Task<IReadOnlyList<TEntity>> GetAllAsync() => await DbSet.ToListAsync();

    public async Task<IReadOnlyList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate) =>
        await DbSet.Where(predicate).ToListAsync();

    public IQueryable<TEntity> Query() => DbSet.AsQueryable();

    public async Task AddAsync(TEntity entity) => await DbSet.AddAsync(entity);

    public void Update(TEntity entity) => DbSet.Update(entity);

    public void Remove(TEntity entity) => DbSet.Remove(entity);
}
