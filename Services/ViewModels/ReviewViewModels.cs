namespace UnifiedFreelanceArtisansDirectory.Services.ViewModels;

public class ReviewDisplayViewModel
{
    public int Id { get; set; }

    public string ClientName { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }

    public string? ServiceProviderReply { get; set; }

    public DateTime? RepliedOn { get; set; }
}

public class ReviewCreateInput
{
    public int ServiceProviderProfileId { get; set; }

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;
}

/// <summary>Unambiguous projection for the admin reviews oversight page — names both parties explicitly.</summary>
public class AdminReviewViewModel
{
    public int Id { get; set; }

    public string ClientName { get; set; } = string.Empty;

    public string ServiceProviderName { get; set; } = string.Empty;

    public int ServiceProviderProfileId { get; set; }

    public int Rating { get; set; }

    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedOn { get; set; }
}
