using Movies.Contracts.Responses.V1;

namespace Movies.Contracts.Responses;

public class MovieRatingsResponse
{
    public required IEnumerable<MovieRatingResponse> Items { get; init; }
}
