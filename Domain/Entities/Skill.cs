namespace UnifiedFreelanceArtisansDirectory.Domain.Entities;

/// <summary>
/// A specific, searchable competency (e.g. "Cabinet Making", "Solar Panel
/// Wiring") that belongs to one category and can be attached to many
/// service provider profiles through ServiceProviderSkill.
/// </summary>
public class Skill
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public ICollection<ServiceProviderSkill> ServiceProviderSkills { get; set; } = new List<ServiceProviderSkill>();
}
