using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public interface IRatingRepository
{
    Task<bool> RateMovieAsync(Guid movieId, Guid userId, int rating, CancellationToken cancellationToken = default);
    Task<float?> GetRatingAsync(Guid movieId, CancellationToken cancellationToken = default);
    Task<(float? AverageRating, int UserRating)> GetRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MovieRating>> GetUserRatingsAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class RatingRepository : IRatingRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public RatingRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> RateMovieAsync(Guid movieId, Guid userId, int rating, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var result = await connection.ExecuteAsync(new("""
            insert into ratings (userId, movieId, rating)
            values (@userId, @movieId, @rating)
            on conflict (userId, movieId) 
                do update
                set rating = @rating;
            """, new { userId, movieId, rating }, cancellationToken: cancellationToken));
        
        return result > 0;
    }
    
    public async Task<float?> GetRatingAsync(Guid movieId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        
        return await connection.QuerySingleOrDefaultAsync<float?>(
            new("""
                select round(avg(r.rating), 1)
                from ratings r
                where movieId = @movieId;
                """, new { movieId }, cancellationToken: cancellationToken));
    }
    
    public async Task<(float? AverageRating, int UserRating)> GetRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        
        return await connection.QuerySingleOrDefaultAsync<(float? AverageRating, int UserRating)>(
            new("""
                select 
                    round(avg(rating), 1),
                    (select rating 
                     from ratings
                     where movieId = @movieId 
                       and userId = @userId 
                     limit 1)
                from ratings
                where movieId = @movieId
                """, new { movieId, userId }, cancellationToken: cancellationToken));
    }
    
    public async Task<bool> DeleteRatingAsync(Guid movieId, Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var result = await connection.ExecuteAsync(new("""
            delete from ratings
            where movieId = @movieId
              and userId = @userId;
            """, new { movieId, userId }, cancellationToken: cancellationToken));
        
        return result > 0;
    }
    public async Task<IEnumerable<MovieRating>> GetUserRatingsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        return await connection.QueryAsync<MovieRating>(
            new("""
                select 
                    m.id as MovieId,
                    m.slug as Slug,
                    r.rating as Rating
                from ratings r
                inner join movies m on m.id = r.movieId
                where r.userId = @userId;
                """, new { userId }, cancellationToken: cancellationToken));
    }
}
