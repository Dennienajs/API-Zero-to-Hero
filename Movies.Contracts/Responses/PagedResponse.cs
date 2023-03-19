namespace Movies.Contracts.Responses;

public class PagedResponse<T>
{
    public required IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
    
    public required int Page { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
