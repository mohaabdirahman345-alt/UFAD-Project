using UnifiedFreelanceArtisansDirectory.Domain.Entities;

namespace UnifiedFreelanceArtisansDirectory.Services;

public interface IServiceProviderCVService
{
    /// <summary>
    /// Gets the CV for a specific Service Provider, if it exists.
    /// </summary>
    Task<ServiceProviderCV?> GetByServiceProviderProfileIdAsync(int serviceProviderProfileId);

    /// <summary>
    /// Uploads a new CV or replaces the existing one for a Service Provider.
    /// Returns the newly created or updated ServiceProviderCV entity.
    /// Throws exceptions for validation failures (invalid file type, size, etc.).
    /// </summary>
    Task<ServiceProviderCV> UploadCVAsync(
        int serviceProviderProfileId,
        IFormFile file,
        string storagePath);

    /// <summary>
    /// Deletes the CV associated with a Service Provider.
    /// Also attempts to delete the physical file from storage.
    /// </summary>
    Task DeleteCVAsync(int serviceProviderProfileId);

    /// <summary>
    /// Gets the full physical file path for a CV document.
    /// Used to serve the file for download/viewing.
    /// </summary>
    Task<string?> GetCVFilePathAsync(int cvId);

    /// <summary>
    /// Validates a CV file before upload.
    /// Checks extension, MIME type, and file size.
    /// Throws an exception if validation fails.
    /// </summary>
    void ValidateCVFile(IFormFile file, long maxFileSize = 10 * 1024 * 1024);
}
