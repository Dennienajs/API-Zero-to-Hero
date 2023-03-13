namespace Movies.Application.Models;

public class GetAllMoviesOptions
{
    public required Guid? UserId { get; init; }
    public required string? Title { get; init; }
    public required int? YearOfRelease { get; init; }
}
