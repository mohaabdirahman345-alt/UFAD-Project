namespace UnifiedFreelanceArtisansDirectory.Domain.Entities;

/// <summary>
/// A client's bookmark of a service provider for quick access later, shown on the
/// client's dashboard. Pure join entity — no attributes beyond the two keys
/// and when the bookmark was made.
/// </summary>
public class Favorite
{
    public int ClientProfileId { get; set; }

    public ClientProfile ClientProfile { get; set; } = null!;

    public int ServiceProviderProfileId { get; set; }

    public ServiceProviderProfile ServiceProviderProfile { get; set; } = null!;

    public DateTime SavedOn { get; set; } = DateTime.UtcNow;
}
