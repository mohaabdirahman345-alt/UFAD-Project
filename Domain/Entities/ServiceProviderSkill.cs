namespace UnifiedFreelanceArtisansDirectory.Domain.Entities;

/// <summary>
/// Join entity linking a service provider to a skill they've claimed, with an
/// optional proficiency note. Modeled explicitly (rather than a bare
/// many-to-many) so a proficiency level can be added later without a
/// breaking schema change.
/// </summary>
public class ServiceProviderSkill
{
    public int ServiceProviderProfileId { get; set; }

    public ServiceProviderProfile ServiceProviderProfile { get; set; } = null!;

    public int SkillId { get; set; }

    public Skill Skill { get; set; } = null!;

    public int ProficiencyLevel { get; set; } = 3; // 1 (learning) through 5 (master craftsperson)
}
