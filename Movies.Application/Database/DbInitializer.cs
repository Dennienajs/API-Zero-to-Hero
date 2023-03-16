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


        // await EnsureData();
    }

    private async Task EnsureData(int take = 100)
    {
        var existingMovies = (await _movieRepository.GetAllAsync(new()
        {
            UserId = null,
            Title = null,
            YearOfRelease = null
        })).ToHashSet();
        
        var newMovies = GetMoviesData()
            .Where(m => existingMovies.All(em => em.Id != m.Id))
            .Take(take)
            .ToList();


        if (!newMovies.Any())
            return;
        
        Console.WriteLine($"Found {existingMovies.Count} existing movies");
        Console.WriteLine($"Creating {newMovies.Count} new movies");


        var counter = 0;
        
        foreach (var movie in newMovies)
        {
            counter++;
            
            if (existingMovies.Any(x => x.Slug == movie.Slug))
            {
                Console.WriteLine($"Movie with slug {movie.Slug} already exists");
                continue;
            }
            
            await _movieRepository.CreateAsync(movie);
            Console.WriteLine($"Created movie: {movie.Title}, {movie.YearOfRelease} ({counter}/{newMovies.Count})");
            
            existingMovies.Add(movie);
        }
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
