using System.Data;
using Dapper;

namespace Movies.Application.Database;

public class DbInitializer
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    
    public DbInitializer(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task InitializeAsync()
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync();
        
        await connection.ExecuteAsync(""" 
            CREATE TABLE IF NOT EXISTS movies (
                id uuid PRIMARY KEY,
                title text NOT NULL,
                slug text NOT NULL,
                yearOfRelease int NOT NULL);
        """);

        await connection.ExecuteAsync("""
            create unique index concurrently if not exists movies_slug_idx
                on movies
                using btree(slug);
        """);
        
        await connection.ExecuteAsync(""" 
            CREATE TABLE IF NOT EXISTS genres (
                movieId uuid references movies(Id),
                name text NOT NULL);
        """);
    }
}
