namespace UnifiedFreelanceArtisansDirectory.Services.ViewModels;

public class PagedResult<TItem>
{
    public IReadOnlyList<TItem> Items { get; init; } = new List<TItem>();

    public int TotalCount { get; init; }

    public int PageNumber { get; init; }

    public int PageSize { get; init; }

    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;
}
