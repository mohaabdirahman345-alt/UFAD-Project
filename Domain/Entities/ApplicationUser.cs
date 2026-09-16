using Microsoft.AspNetCore.Identity;

namespace UnifiedFreelanceArtisansDirectory.Domain.Entities;

/// <summary>
/// Extends the framework's identity user with the profile fields shared by
/// every account in the system, regardless of role. Role-specific data lives
/// in ServiceProviderProfile and ClientProfile, each linked back to this record.
/// </summary>
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? ProfileImagePath { get; set; }

    public DateTime RegisteredOn { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Administrators can deactivate an account without deleting it, which
    /// preserves reviews and portfolio history tied to the user.
    /// </summary>
    public bool IsActive { get; set; } = true;

    public string FullName => $"{FirstName} {LastName}".Trim();

    public ServiceProviderProfile? ServiceProviderProfile { get; set; }

    public ClientProfile? ClientProfile { get; set; }
}
