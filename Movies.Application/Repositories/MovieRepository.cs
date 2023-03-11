using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public interface IMovieRepository
{
    Task<bool> CreateAsync(Movie movie);
    Task<Movie?> GetByIdAsync(Guid id);
    Task<Movie?> GetBySlugAsync(string slug);
    Task<IEnumerable<Movie>> GetAllAsync();
    Task<bool> UpdateAsync(Movie movie);
    Task<bool> DeleteByIdAsync(Guid id);
    Task<bool> ExistsByIdAsync(Guid id);
}

public class MovieRepository : IMovieRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public MovieRepository(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }
    
    public async Task<bool> CreateAsync(Movie movie)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();

        var result = await connection.ExecuteAsync(new("""
            insert into movies (id, title, slug, yearofrelease)
            values (@Id, @Title, @Slug, @YearOfRelease);
            """, movie));

        if (result > 0)
        {
            foreach (var genre in movie.Genres)
            {
                await connection.ExecuteAsync(new("""
                    insert into genres (movieId, name)
                    values (@MovieId, @Name);
                    """, new { MovieId = movie.Id, Name = genre }));
            }
        }
        transaction.Commit();
        
        return result > 0;
    }
    
    public async Task<Movie?> GetByIdAsync(Guid id)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new("""
            select * from movies where id = @Id;
            """, new { id }));

        if (movie is null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(
            new("""
            select name from genres where movieId = @Id;
            """, new { id }));

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }
    
    public async Task<Movie?> GetBySlugAsync(string slug)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        var movie = await connection.QuerySingleOrDefaultAsync<Movie>(
            new("""
            select * from movies where slug = @Slug;
            """, new { slug }));

        if (movie is null)
        {
            return null;
        }

        var genres = await connection.QueryAsync<string>(
            new("""
            select name from genres where movieId = @Id;
            """, new { id = movie.Id }));

        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        return movie;
    }
    
    public async Task<IEnumerable<Movie>> GetAllAsync()
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        var result = await connection.QueryAsync(new("""
            select m.*, string_agg(g.name, ',') as genres
            from movies m
            left join genres g on g.movieId = m.id
            group by id
            """));

        return result.Select(x => new Movie
        {
            Id = x.id,
            Title = x.title,
            YearOfRelease = x.yearofrelease,
            Genres = Enumerable.ToList(x.genres.Split(','))
        });
    }
    
    public async Task<bool> UpdateAsync(Movie movie)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();
        
        await connection.ExecuteAsync(new("""
            delete from genres 
            where movieId = @Id
            """, new { movie.Id }));
 
        foreach (var name in movie.Genres)
        {
            await connection.ExecuteAsync(new("""
                insert into genres (movieId, name)
                values (@Id, @Name);
                """, new { movie.Id, name }));
        }
        
        var result = await connection.ExecuteAsync(new("""
            update movies 
            set title = @Title, 
                slug = @Slug, 
                yearofrelease = @YearOfRelease
            where id = @Id;
            """, movie));
        
        transaction.Commit();
        
        return result > 0;
    }
    
    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        using var transaction = connection.BeginTransaction();
        
        await connection.ExecuteAsync(new("""
            delete from genres where movieId = @Id;
            """, new { id }));
        
        var result = await connection.ExecuteAsync(new("""
            delete from movies where id = @Id
            """, new { id }));
        
        transaction.Commit();
        
        return result > 0;
    }
    
    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();

        return await connection.ExecuteScalarAsync<bool>(new("""
            select count(1) from movies where id = @Id;
            """, new { id }));
    }
}
