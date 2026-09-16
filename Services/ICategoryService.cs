using UnifiedFreelanceArtisansDirectory.Domain.Entities;

namespace UnifiedFreelanceArtisansDirectory.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<Category>> GetActiveAsync();

    Task<IReadOnlyList<Category>> GetAllAsync();

    Task<Category?> GetByIdAsync(int id);

    Task<Category> CreateAsync(string name, string description, string iconClass);

    Task UpdateAsync(int id, string name, string description, string iconClass);

    Task SetActiveAsync(int id, bool isActive);
}
