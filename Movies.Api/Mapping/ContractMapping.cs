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

    public static MoviesResponse MapToResponse(this IEnumerable<Movie> movies, 
        int page, 
        int pageSize, 
        int totalCount) => new()
    {
        Items = movies.Select(movie => movie.MapToResponse()),
        Page = page,
        PageSize = pageSize,
        TotalCount = totalCount
    };

    public static MoviesResponse MapToResponse(this IEnumerable<Movie> movies, 
        GetAllMoviesRequest request, 
        int totalCount) =>
        movies.MapToResponse(
            request.Page.GetValueOrDefault(PagedRequest.DefaultPage), 
            request.PageSize.GetValueOrDefault(PagedRequest.DefaultPageSize), 
            totalCount);

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
        YearOfRelease = request.YearOfRelease,
        
        SortField = request.SortBy?.Trim('+', '-'),
        SortOrder = request.SortBy is null 
            ? SortOrder.Unsorted 
            : request.SortBy.StartsWith('-') 
                ? SortOrder.Descending 
                : SortOrder.Ascending,
        
        Page = request.Page.GetValueOrDefault(PagedRequest.DefaultPage),
        PageSize = request.PageSize.GetValueOrDefault(PagedRequest.DefaultPageSize)
    };
}
