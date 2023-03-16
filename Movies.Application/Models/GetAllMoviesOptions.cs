namespace Movies.Application.Models;

public class GetAllMoviesOptions
{
    public required Guid? UserId { get; init; }
    public required string? Title { get; init; }
    public required int? YearOfRelease { get; init; }
    
    public string? SortField { get; init; }
    public SortOrder SortOrder { get; init; } = SortOrder.Unsorted;
}

public enum SortOrder
{
    Unsorted,
    Ascending,
    Descending
}
