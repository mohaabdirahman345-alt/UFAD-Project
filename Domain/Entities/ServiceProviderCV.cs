namespace UnifiedFreelanceArtisansDirectory.Domain.Entities;

/// <summary>
/// Stores a Service Provider's CV/Resume document as part of their professional profile.
/// The CV helps Administrators evaluate the Service Provider's qualifications during approval.
/// A Service Provider can have zero or one current CV.
/// </summary>
public class ServiceProviderCV
{
    public int Id { get; set; }

    public int ServiceProviderProfileId { get; set; }

    public ServiceProviderProfile ServiceProviderProfile { get; set; } = null!;

    /// <summary>
    /// The original filename provided by the user (e.g., "John_Doe_CV.pdf").
    /// Displayed to the user for reference.
    /// </summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>
    /// The unique server-side filename used for storage (e.g., "cv_12345_abc123def456.pdf").
    /// Prevents directory traversal and filename collisions.
    /// </summary>
    public string StoredFileName { get; set; } = string.Empty;

    /// <summary>
    /// The relative file path from the webroot (e.g., "/uploads/cvs/cv_12345_abc123def456.pdf").
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// The MIME type of the uploaded file (e.g., "application/pdf").
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// The size of the uploaded file in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// When the CV was first uploaded.
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the CV was last updated (either via replacement or metadata changes).
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
