using Movies.Application.Models;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Mapping;

public static class ContractMapping
{
    public static Movie MapToMovie(this CreateMovieRequest request) => new()
    {
        Id = Guid.NewGuid(),
        Title = request.Title,
        YearOfRelease = request.YearOfRelease,
        Genres = request.Genres.ToList()
    };

    public static MovieResponse MapToResponse(this Movie movie) => new()
    {
        Id = movie.Id,
        Title = movie.Title,
        Slug = movie.Slug,
        AverageRating = movie.AverageRating,
        UserRating = movie.UserRating,
        YearOfRelease = movie.YearOfRelease,
        Genres = movie.Genres
    };

    public static MoviesResponse MapToResponse(this IEnumerable<Movie> movies) => new()
    {
        Items = movies.Select(movie => movie.MapToResponse())
    };
    
    public static Movie MapToMovie(this UpdateMovieRequest request, Guid id) => new()
    {
        Id = id,
        Title = request.Title,
        YearOfRelease = request.YearOfRelease,
        Genres = request.Genres.ToList()
    };
    
    public static MovieRatingsResponse MapToResponse(this IEnumerable<MovieRating> movieRatings) => new()
    {
        Items = movieRatings.Select(MapToResponse)
    };
    
    public static MovieRatingResponse MapToResponse(this MovieRating movieRating) => new()
    {
        MovieId = movieRating.MovieId,
        Slug = movieRating.Slug,
        Rating = movieRating.Rating
    };
    
    public static GetAllMoviesOptions MapToOptions(this GetAllMoviesRequest request, Guid? userId = null) => new()
    {
        UserId = userId,
        Title = request.Title,
        YearOfRelease = request.YearOfRelease
    };
}
