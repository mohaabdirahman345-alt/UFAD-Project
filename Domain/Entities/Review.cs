namespace UnifiedFreelanceArtisansDirectory.Domain.Entities;

/// <summary>
/// A client's rating and comment for a service provider, forming the reputation
/// signal the whole directory is built around. A client may leave only one
/// review per service provider, enforced by a unique index in the DbContext.
/// </summary>
public class Review
{
    public int Id { get; set; }

    public int ServiceProviderProfileId { get; set; }

    public ServiceProviderProfile ServiceProviderProfile { get; set; } = null!;

    public int ClientProfileId { get; set; }

    public ClientProfile ClientProfile { get; set; } = null!;

    /// <summary>1 through 5.</summary>
    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public DateTime? EditedOn { get; set; }

    /// <summary>
    /// A service provider's reply to the review, shown beneath it on their profile.
    /// </summary>
    public string? ServiceProviderReply { get; set; }

    public DateTime? RepliedOn { get; set; }
}
