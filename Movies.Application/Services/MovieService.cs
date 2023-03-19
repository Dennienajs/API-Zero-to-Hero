using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services;

public interface IMovieService
{
    Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default);
    Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken cancellationToken = default);
    Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken cancellationToken = default);
    Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken cancellationToken = default);
    Task<Movie?> UpdateAsync(Movie movie, Guid? userId = default, CancellationToken cancellationToken = default);
    Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(string? title, int? yearOfRelease, CancellationToken cancellationToken = default);
}

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly IRatingRepository _ratingRepository;
    private readonly IValidator<Movie> _movieValidator;
    private readonly IValidator<GetAllMoviesOptions> _getAllMoviesOptionsValidator;

    public MovieService(
        IMovieRepository movieRepository, 
        IValidator<Movie> movieValidator, 
        IRatingRepository ratingRepository, 
        IValidator<GetAllMoviesOptions> getAllMoviesOptionsValidator)
    {
        _movieRepository = movieRepository;
        _movieValidator = movieValidator;
        _ratingRepository = ratingRepository;
        _getAllMoviesOptionsValidator = getAllMoviesOptionsValidator;
    }
    
    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken);
        return await _movieRepository.CreateAsync(movie, cancellationToken);
    }
    
    public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken cancellationToken = default)
    {
        return await _movieRepository.GetByIdAsync(id, userId, cancellationToken);
    }
    
    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken cancellationToken = default)
    {
        return await _movieRepository.GetBySlugAsync(slug, userId, cancellationToken);
    }
    
    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken cancellationToken = default)
    {
        await _getAllMoviesOptionsValidator.ValidateAndThrowAsync(options, cancellationToken);
        
        return await _movieRepository.GetAllAsync(options, cancellationToken);
    }
    
    public async Task<Movie?> UpdateAsync(Movie movie, Guid? userId = default, CancellationToken cancellationToken = default)
    {
        await _movieValidator.ValidateAndThrowAsync(movie, cancellationToken);

        if (!await _movieRepository.ExistsByIdAsync(movie.Id, cancellationToken))
            return null;

        await _movieRepository.UpdateAsync(movie, cancellationToken);

        if (!userId.HasValue)
        {
            movie.AverageRating = await _ratingRepository.GetRatingAsync(movie.Id, cancellationToken);
            return movie;
        }

        var ratings = await _ratingRepository.GetRatingAsync(movie.Id, userId.Value, cancellationToken);
        movie.AverageRating = ratings.AverageRating;
        movie.UserRating = ratings.UserRating;
        return movie;
    }
    
    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _movieRepository.DeleteByIdAsync(id, cancellationToken);
    }
    
    public async Task<int> CountAsync(string? title, int? yearOfRelease, CancellationToken cancellationToken = default)
    {
        return await _movieRepository.CountAsync(title, yearOfRelease, cancellationToken);
    }
}
