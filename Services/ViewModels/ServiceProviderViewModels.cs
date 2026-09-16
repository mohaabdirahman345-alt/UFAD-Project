using UnifiedFreelanceArtisansDirectory.Domain.Enums;

namespace UnifiedFreelanceArtisansDirectory.Services.ViewModels;

/// <summary>Card-level projection used on the browse/search results page.</summary>
public class ServiceProviderSummaryViewModel
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? ProfileImagePath { get; set; }

    public string Headline { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public decimal? HourlyRate { get; set; }

    public bool IsVerified { get; set; }

    public ApprovalStatus ApprovalStatus { get; set; }

    public AvailabilityStatus AvailabilityStatus { get; set; }

    public double AverageRating { get; set; }

    public int ReviewCount { get; set; }

    public IReadOnlyList<string> TopSkills { get; set; } = new List<string>();
}

/// <summary>Full projection for a service provider's public profile page.</summary>
public class ServiceProviderDetailViewModel
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? ProfileImagePath { get; set; }

    public string Headline { get; set; } = string.Empty;

    public string Bio { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int YearsOfExperience { get; set; }

    public decimal? HourlyRate { get; set; }

    public bool IsVerified { get; set; }

    public ApprovalStatus ApprovalStatus { get; set; }

    public AvailabilityStatus AvailabilityStatus { get; set; }

    public int ProfileViews { get; set; }

    public int ProfileCompletionPercentage { get; set; }

    public double AverageRating { get; set; }

    public int ReviewCount { get; set; }

    public IReadOnlyList<string> Skills { get; set; } = new List<string>();

    public IReadOnlyList<PortfolioItemViewModel> PortfolioItems { get; set; } = new List<PortfolioItemViewModel>();

    public IReadOnlyList<ReviewDisplayViewModel> Reviews { get; set; } = new List<ReviewDisplayViewModel>();
}

public class ServiceProviderProfileEditInput
{
    public string Headline { get; set; } = string.Empty;

    public string Bio { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public int YearsOfExperience { get; set; }

    public decimal? HourlyRate { get; set; }

    public string Address { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public AvailabilityStatus AvailabilityStatus { get; set; } = AvailabilityStatus.Available;

    public List<int> SkillIds { get; set; } = new();
}

public class PortfolioItemViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ImagePath { get; set; } = string.Empty;

    public DateTime CompletedOn { get; set; }
}
