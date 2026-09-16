namespace UnifiedFreelanceArtisansDirectory.Domain.Entities;

/// <summary>
/// A top-level trade or service grouping (e.g. Carpentry, Tailoring,
/// Electrical Work) used to organize service provider profiles and drive the
/// public directory's browse and filter experience.
/// </summary>
public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// CSS class name for the icon shown on category tiles and filter chips.
    /// </summary>
    public string IconClass { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<ServiceProviderProfile> ServiceProviderProfiles { get; set; } = new List<ServiceProviderProfile>();

    public ICollection<Skill> Skills { get; set; } = new List<Skill>();
}
