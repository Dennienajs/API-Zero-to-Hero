using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public interface IMovieRepository
{
    Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default);
    Task<Movie?> GetByIdAsync(Guid id, Guid? userId = default, CancellationToken cancellationToken = default);
    Task<Movie?> GetBySlugAsync(string slug, Guid? userId = default, CancellationToken cancellationToken = default);
    Task<IEnumerable<Movie>> GetAllAsync(Guid? userId = default, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Movie movie, CancellationToken cancellationToken = default);
    Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default);
}

public class MovieRepository : IMovieRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MovieRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }
    
    public async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        var result = await connection.ExecuteAsync(new("""
            insert into movies (id, title, slug, yearofrelease)
            values (@Id, @Title, @Slug, @YearOfRelease);
            """, movie, cancellationToken: cancellationToken));

        if (result > 0)
        {
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new("""
                    insert into genres (movieId, name)
                    values (@MovieId, @Name);
                    """, new { MovieId = movie.Id, Name = genre }, cancellationToken: cancellationToken));
            }
        }
        transaction.Commit();
        
        return result > 0;
    }
    
    public async Task<Movie?> GetByIdAsync(Guid id, Guid? userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new("""
            select 
                    m.*,
                    round(avg(r.rating), 1) as averageRating,
                    myRatings.rating as userRating
            from movies m
            left join ratings r on r.movieId = m.id
            left join ratings myRatings on myRatings.movieId = m.id
                and myRatings.userId = @UserId
            where id = @Id
            group by id, userRating;
            """, new { id, userId }, cancellationToken: cancellationToken));

        if (movie is null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(
            new("""
            select name from genres where movieId = @Id;
            """, new { id }, cancellationToken: cancellationToken));

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }
    
    public async Task<Movie?> GetBySlugAsync(string slug, Guid? userId, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new("""
            select 
                    m.*,
                    round(avg(r.rating), 1) as averageRating,
                    myRatings.rating as userRating
            from movies m
            left join ratings r on r.movieId = m.id
            left join ratings myRatings on myRatings.movieId = m.id
                and myRatings.userId = @UserId
            where slug = @slug
            group by id, userRating;
            """, new { slug, userId }, cancellationToken: cancellationToken));

        if (movie is null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(
            new("""
            select name from genres where movieId = @Id;
            """, new { id = movie.Id }, cancellationToken: cancellationToken));

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }
    

    public async Task<IEnumerable<Movie>> GetAllAsync(Guid? userId = default, CancellationToken token = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
        
        var result = await connection.QueryAsync(new("""
            select m.*, 
                   string_agg(distinct g.name, ',') as genres , 
                   round(avg(r.rating), 1) as averagerating, 
                   myr.rating as userrating
            from movies m 
            left join genres g on m.id = g.movieId
            left join ratings r on m.id = r.movieId
            left join ratings myr on m.id = myr.movieId
                and myr.userid = @userId
            group by id, userRating
            """, new { userId }, cancellationToken: token));
        
        return result.Select(x => new Movie
        {
            Id = x.id,
            Title = x.title,
            YearOfRelease = x.yearofrelease,
            AverageRating = (float?) x.averagerating,
            UserRating = (int?) x.userrating,
            Genres = Enumerable.ToList(x.genres.Split(','))
        });
    }
    
    public async Task<bool> UpdateAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();
        
        await connection.ExecuteAsync(new("""
            delete from genres 
            where movieId = @Id
            """, new { movie.Id }, cancellationToken: cancellationToken));
 
        foreach (var name in movie.Genres)
        {
            await connection.ExecuteAsync(new("""
                insert into genres (movieId, name)
                values (@Id, @Name);
                """, new { movie.Id, name }, cancellationToken: cancellationToken));
        }
        
        var result = await connection.ExecuteAsync(new("""
            update movies 
            set title = @Title, 
                slug = @Slug, 
                yearofrelease = @YearOfRelease
            where id = @Id;
            """, movie, cancellationToken: cancellationToken));
        
        transaction.Commit();
        
        return result > 0;
    }
    
    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();
        
        await connection.ExecuteAsync(new("""
            delete from genres where movieId = @Id;
            """, new { id }, cancellationToken: cancellationToken));
        
        var result = await connection.ExecuteAsync(new("""
            delete from movies where id = @Id
            """, new { id }, cancellationToken: cancellationToken));
        
        transaction.Commit();
        
        return result > 0;
    }
    
    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(new("""
            select count(1) from movies where id = @Id;
            """, new { id }, cancellationToken: cancellationToken));
    }
}
