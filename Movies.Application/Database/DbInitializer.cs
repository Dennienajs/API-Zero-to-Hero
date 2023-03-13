using System.Data;
using System.Text.Json;
using Dapper;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Database;

public class DbInitializer
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IMovieRepository _movieRepository;
    
    public DbInitializer(IDbConnectionFactory dbConnectionFactory, IMovieRepository movieRepository)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _movieRepository = movieRepository;
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
        
        await connection.ExecuteAsync(""" 
            CREATE TABLE IF NOT EXISTS ratings (
                userId uuid,
                movieId uuid references movies(Id),
                rating int NOT NULL,
                PRIMARY KEY (userId, movieId));
        """);


        await EnsureData();
    }

    private async Task EnsureData()
    {
        var existingMovies = await _movieRepository.GetAllAsync(new()
        {
            UserId = null,
            Title = null,
            YearOfRelease = null
        });

        var newMovies = GetMoviesData().Where(m => existingMovies.All(em => em.Id != m.Id)).Take(100).ToList();

        if (!newMovies.Any())
            return;
        
        
        Console.WriteLine($"Creating {newMovies.Count} movies ...");
        
        foreach (var movie in newMovies)
        {
            await _movieRepository.CreateAsync(movie);
            Console.WriteLine($"Created movie: {movie.Title}, {movie.YearOfRelease}");
        }
        
        Console.WriteLine($"Created {newMovies.Count} movies!");

    }

    private static IEnumerable<Movie> GetMoviesData()
    {
        var current = Directory.GetCurrentDirectory();
        var src = Directory.GetParent(current)!.FullName;
        var path = Path.Combine(src, "Helpers", "movies.json");
        
        var data = File.ReadAllText(path);
        var movies =  JsonSerializer.Deserialize<IEnumerable<Movie>>(data);

        return movies ?? Enumerable.Empty<Movie>();
    }
}
