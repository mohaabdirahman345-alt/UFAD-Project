using Microsoft.EntityFrameworkCore;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Repositories;

namespace UnifiedFreelanceArtisansDirectory.Services;

public class SkillService : ISkillService
{
    private readonly IUnitOfWork _unitOfWork;

    public SkillService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<Skill>> GetByCategoryAsync(int categoryId) =>
        await _unitOfWork.Skills.Query()
            .Where(s => s.CategoryId == categoryId)
            .OrderBy(s => s.Name)
            .ToListAsync();

    public async Task<IReadOnlyList<Skill>> GetAllAsync() =>
        await _unitOfWork.Skills.Query()
            .Include(s => s.Category)
            .OrderBy(s => s.Category.Name)
            .ThenBy(s => s.Name)
            .ToListAsync();

    public async Task<Skill> CreateAsync(string name, int categoryId)
    {
        var skill = new Skill { Name = name.Trim(), CategoryId = categoryId };

        await _unitOfWork.Skills.AddAsync(skill);
        await _unitOfWork.SaveChangesAsync();

        return skill;
    }

    public async Task DeleteAsync(int id)
    {
        var skill = await _unitOfWork.Skills.GetByIdAsync(id);
        if (skill is null)
        {
            return;
        }

        _unitOfWork.Skills.Remove(skill);
        await _unitOfWork.SaveChangesAsync();
    }
}
