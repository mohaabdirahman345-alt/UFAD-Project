namespace UnifiedFreelanceArtisansDirectory.Domain.Entities;

/// <summary>
/// The profile for an account that hires service providers. Kept
/// deliberately lighter than ServiceProviderProfile since clients are not
/// searchable entries in the directory themselves.
/// </summary>
public class ClientProfile
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public ApplicationUser User { get; set; } = null!;

    public string Neighborhood { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    public ICollection<Review> Reviews { get; set; } = new List<Review>();

    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();
}
