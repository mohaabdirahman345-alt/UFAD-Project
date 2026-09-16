namespace UnifiedFreelanceArtisansDirectory.Domain.Enums;

/// <summary>
/// The fixed set of reasons a user can cite when reporting a profile or
/// review, kept as an enum (rather than free text) so administrators can
/// filter and triage the report queue by category.
/// </summary>
public enum ReportReason
{
    Impersonation = 0,
    Fraud = 1,
    Harassment = 2,
    Spam = 3,
    InappropriateContent = 4,
    QualityDispute = 5,
    Other = 6
}
