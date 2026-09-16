namespace UnifiedFreelanceArtisansDirectory.Domain.Entities;

/// <summary>
/// A single piece of completed work a service provider showcases on their public
/// profile — a finished cabinet, a wiring job, a tailored garment.
/// </summary>
public class PortfolioItem
{
    public int Id { get; set; }

    public int ServiceProviderProfileId { get; set; }

    public ServiceProviderProfile ServiceProviderProfile { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ImagePath { get; set; } = string.Empty;

    public DateTime CompletedOn { get; set; }

    public DateTime UploadedOn { get; set; } = DateTime.UtcNow;

    public int DisplayOrder { get; set; }
}
