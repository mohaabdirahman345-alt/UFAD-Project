using UnifiedFreelanceArtisansDirectory.Domain.Entities;
using UnifiedFreelanceArtisansDirectory.Repositories;

namespace UnifiedFreelanceArtisansDirectory.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<Category>> GetActiveAsync() =>
        (await _unitOfWork.Categories.FindAsync(c => c.IsActive))
            .OrderBy(c => c.Name)
            .ToList();

    public async Task<IReadOnlyList<Category>> GetAllAsync() =>
        (await _unitOfWork.Categories.GetAllAsync())
            .OrderBy(c => c.Name)
            .ToList();

    public Task<Category?> GetByIdAsync(int id) => _unitOfWork.Categories.GetByIdAsync(id);

    public async Task<Category> CreateAsync(string name, string description, string iconClass)
    {
        var category = new Category
        {
            Name = name.Trim(),
            Description = description.Trim(),
            IconClass = iconClass.Trim(),
            IsActive = true
        };

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return category;
    }

    public async Task UpdateAsync(int id, string name, string description, string iconClass)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"Category {id} was not found.");

        category.Name = name.Trim();
        category.Description = description.Trim();
        category.IconClass = iconClass.Trim();

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SetActiveAsync(int id, bool isActive)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id)
            ?? throw new InvalidOperationException($"Category {id} was not found.");

        category.IsActive = isActive;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();
    }
}
