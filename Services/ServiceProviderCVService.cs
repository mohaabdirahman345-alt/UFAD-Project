using Microsoft.EntityFrameworkCore;
using UnifiedFreelanceArtisansDirectory.Data;
using UnifiedFreelanceArtisansDirectory.Domain.Entities;

namespace UnifiedFreelanceArtisansDirectory.Services;

public class ServiceProviderCVService : IServiceProviderCVService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    // Allowed file extensions (case-insensitive)
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
        ".doc",
        ".docx"
    };

    // Allowed MIME types
    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-word.document.macroEnabled.12",
        "application/vnd.ms-word.template.macroEnabled.12"
    };

    public ServiceProviderCVService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<ServiceProviderCV?> GetByServiceProviderProfileIdAsync(int serviceProviderProfileId)
    {
        return await _context.ServiceProviderCVs
            .FirstOrDefaultAsync(cv => cv.ServiceProviderProfileId == serviceProviderProfileId);
    }

    public async Task<ServiceProviderCV> UploadCVAsync(
        int serviceProviderProfileId,
        IFormFile file,
        string storagePath)
    {
        // Validate the file
        ValidateCVFile(file);

        // Verify the Service Provider profile exists
        var profile = await _context.ServiceProviderProfiles.FindAsync(serviceProviderProfileId);
        if (profile is null)
        {
            throw new ArgumentException($"Service Provider profile with ID {serviceProviderProfileId} not found.");
        }

        // Get existing CV (if any) to delete it
        var existingCV = await GetByServiceProviderProfileIdAsync(serviceProviderProfileId);
        if (existingCV is not null)
        {
            await DeleteCVAsync(serviceProviderProfileId);
        }

        // Generate a unique server-side filename to prevent collisions and directory traversal
        var uniqueFileName = GenerateUniqueFileName(file.FileName, serviceProviderProfileId);
        var relativePath = $"/uploads/cvs/{uniqueFileName}";
        var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cvs", uniqueFileName);

        // Ensure the upload directory exists
        var uploadDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cvs");
        Directory.CreateDirectory(uploadDirectory);

        // Save the file to disk
        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Get the file info for metadata
        var fileInfo = new FileInfo(fullPath);

        // Create and save the database record
        var cv = new ServiceProviderCV
        {
            ServiceProviderProfileId = serviceProviderProfileId,
            OriginalFileName = file.FileName,
            StoredFileName = uniqueFileName,
            FilePath = relativePath,
            ContentType = file.ContentType ?? "application/octet-stream",
            FileSize = fileInfo.Length,
            UploadedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ServiceProviderCVs.Add(cv);
        await _context.SaveChangesAsync();

        return cv;
    }

    public async Task DeleteCVAsync(int serviceProviderProfileId)
    {
        var cv = await GetByServiceProviderProfileIdAsync(serviceProviderProfileId);
        if (cv is null)
        {
            return; // Nothing to delete
        }

        // Delete the physical file from storage
        try
        {
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cvs", cv.StoredFileName);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't throw — the database record should still be removed
            Console.WriteLine($"Error deleting CV file {cv.StoredFileName}: {ex.Message}");
        }

        // Delete the database record
        _context.ServiceProviderCVs.Remove(cv);
        await _context.SaveChangesAsync();
    }

    public async Task<string?> GetCVFilePathAsync(int cvId)
    {
        var cv = await _context.ServiceProviderCVs.FindAsync(cvId);
        if (cv is null)
        {
            return null;
        }

        var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cvs", cv.StoredFileName);

        // Verify the file actually exists before returning the path
        if (File.Exists(fullPath))
        {
            return fullPath;
        }

        return null;
    }

    public void ValidateCVFile(IFormFile file, long maxFileSize = 10 * 1024 * 1024)
    {
        if (file is null || file.Length == 0)
        {
            throw new InvalidOperationException("Please select a CV file to upload.");
        }

        if (file.Length > maxFileSize)
        {
            throw new InvalidOperationException($"The CV file must not exceed {maxFileSize / (1024 * 1024)} MB.");
        }

        // Validate file extension
        var fileExtension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(fileExtension))
        {
            throw new InvalidOperationException("Please upload a PDF, DOC, or DOCX file.");
        }

        // Validate MIME type
        if (!AllowedMimeTypes.Contains(file.ContentType ?? string.Empty))
        {
            throw new InvalidOperationException("The file type is not supported. Please upload a valid PDF or Word document.");
        }

        // Additional validation: check file signature (magic bytes) to prevent spoofed files
        ValidateFileMagicBytes(file);
    }

    private static void ValidateFileMagicBytes(IFormFile file)
    {
        // Read the first few bytes to check the file signature
        using (var stream = file.OpenReadStream())
        {
            byte[] buffer = new byte[10];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            
            if (bytesRead < 2)
            {
                throw new InvalidOperationException("The uploaded file is not valid.");
            }

            // PDF: %PDF
            if (file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                if (!(buffer[0] == 0x25 && buffer[1] == 0x50 && buffer[2] == 0x44 && buffer[3] == 0x46))
                {
                    throw new InvalidOperationException("The uploaded file is not a valid PDF.");
                }
            }

            // DOC/DOCX: Check for MS Office signatures
            if (file.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            {
                // DOCX is a ZIP file with specific structure
                if (!(buffer[0] == 0x50 && buffer[1] == 0x4B)) // PK (ZIP signature)
                {
                    throw new InvalidOperationException("The uploaded file is not a valid DOCX document.");
                }
            }

            if (file.FileName.EndsWith(".doc", StringComparison.OrdinalIgnoreCase))
            {
                // DOC files typically start with D0CF or FE37
                if (!((buffer[0] == 0xD0 && buffer[1] == 0xCF) || (buffer[0] == 0xFE && buffer[1] == 0x37)))
                {
                    // Allow DOC files that start with PK (some newer formats are ZIP-based)
                    if (!(buffer[0] == 0x50 && buffer[1] == 0x4B))
                    {
                        throw new InvalidOperationException("The uploaded file is not a valid Word document.");
                    }
                }
            }
        }
    }

    private static string GenerateUniqueFileName(string originalFileName, int serviceProviderProfileId)
    {
        // Create a unique filename using the profile ID and a random string
        var extension = Path.GetExtension(originalFileName);
        var randomPart = Guid.NewGuid().ToString("N").Substring(0, 12);
        return $"cv_{serviceProviderProfileId}_{randomPart}{extension}";
    }
}
