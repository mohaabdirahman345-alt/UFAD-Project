using UnifiedFreelanceArtisansDirectory.Domain.Entities;

namespace UnifiedFreelanceArtisansDirectory.Services;

public interface ISkillService
{
    Task<IReadOnlyList<Skill>> GetByCategoryAsync(int categoryId);

    Task<IReadOnlyList<Skill>> GetAllAsync();

    Task<Skill> CreateAsync(string name, int categoryId);

    Task DeleteAsync(int id);
}
