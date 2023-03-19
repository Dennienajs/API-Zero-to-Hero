using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Movies.Api.Sdk;
using Movies.Api.Sdk.Consumer;
using Refit;

const string baseUrl = "http://localhost:5072";

ServiceCollection services = new();

services
    .AddLogging((loggingBuilder) => loggingBuilder
        .SetMinimumLevel(LogLevel.Information)
        .AddConsole())
    .AddHttpClient()
    .AddSingleton<AuthTokenProvider>()
    .AddRefitClient<IMoviesApi>(serviceProvider => new()
    {
        AuthorizationHeaderValueGetter = async () => await serviceProvider
            .GetRequiredService<AuthTokenProvider>()
            .GetTokenAsync()
    })
    .ConfigureHttpClient(x => x.BaseAddress = new(baseUrl));

var provider = services.BuildServiceProvider();


try
{
    var moviesApi = provider.GetRequiredService<IMoviesApi>();
    const string id = "03f6c622-e86b-4200-bb73-89107e287cc8";
    const string slug = "bears-beets-battlestar-galactica-2-2024";

    var movieById = await moviesApi.GetMovieAsync(id);
    movieById.WriteToConsole();

    var movieBySlug = await moviesApi.GetMovieAsync(slug);
    movieBySlug.WriteToConsole();

    var movies = await moviesApi.GetAllMoviesAsync(new()
    {
        Title = null,
        YearOfRelease = null,
        SortBy = null,
        Page = 1,
        PageSize = 100
    });
    movies.WriteToConsole();


    var newMovie = await moviesApi.CreateMovieAsync(new()
    {
        Title = "Bears, Beets, Battlestar Galctica 9001",
        YearOfRelease = 2024,
        Genres = new[]
        {
            "Comedy"
        }
    });
    newMovie.WriteToConsole();

    var updatedMovie = await moviesApi.UpdateMovieAsync(newMovie.Id, new()
    {
        Title = "Bears, Beets, Battlestar Galctica 9000",
        YearOfRelease = 2024,
        Genres = new[]
        {
            "Comedy",
            "Drama"
        }
    });
    updatedMovie.WriteToConsole();

    await moviesApi.RateMovieAsync(newMovie.Id, new()
    {
        Rating = 5
    });

    var ratings2 = await moviesApi.GetUserRatingsAsync();
    ratings2.WriteToConsole();

    await moviesApi.DeleteRatingAsync(newMovie.Id);
}
catch (ApiException e)
{
    // This is the exception type thrown by Refit when the API returns a non-success status code.
    Console.WriteLine(e.Content);
}
catch (Exception e)
{
    Console.WriteLine(e);
}