using FluentValidation;
using FluentValidation.Results;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services;

public interface IRatingService
{
    Task<bool> RateMovieAsync(Guid movieId, Guid userId, int rating, CancellationToken cancellationToken = default);
    Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MovieRating>> GetUserRatingsAsync(Guid userId, CancellationToken cancellationToken = default);

}

public class RatingService : IRatingService
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IMovieRepository _movieRepository;

    public RatingService(IRatingRepository ratingRepository, IMovieRepository movieRepository)
    {
        _ratingRepository = ratingRepository;
        _movieRepository = movieRepository;
    }

    public async Task<bool> RateMovieAsync(Guid movieId, Guid userId, int rating, CancellationToken cancellationToken = default)
    {
        if(rating is < 1 or > 5)
            throw new ValidationException(new[]
            {
                new ValidationFailure("Rating", "Rating must be between 1 and 5")
            });

        return 
            await _movieRepository.ExistsByIdAsync(movieId, cancellationToken) 
            && await _ratingRepository.RateMovieAsync(movieId, userId, rating, cancellationToken);
    }
    
    public async Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _ratingRepository.DeleteRatingAsync(movieId, userId, cancellationToken);
    }
    
    public async Task<IEnumerable<MovieRating>> GetUserRatingsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _ratingRepository.GetUserRatingsAsync(userId, cancellationToken);
    }
}
