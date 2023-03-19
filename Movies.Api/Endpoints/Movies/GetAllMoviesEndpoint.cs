using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Endpoints.Movies;

public static class GetAllMoviesEndpoint
{
    public const string Name = "GetMovies";

    public static IEndpointRouteBuilder MapGetAllMovies(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Movies.GetAll, async (
                [AsParameters] GetAllMoviesRequest request,
                IMovieService movieService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                var userId = context.GetUserId();
                var options = request.MapToOptions(userId);

                var movies = await movieService.GetAllAsync(options, cancellationToken);
                var count = await movieService.CountAsync(options.Title, options.YearOfRelease, cancellationToken);

                var response = movies.MapToResponse(request, count);

                return TypedResults.Ok(response);
            })
            .WithName(Name + "V1")
            .Produces<MoviesResponse>(StatusCodes.Status200OK)
            .WithApiVersionSet(ApiVersioning.VersionSet)
            .HasApiVersion(1.0);
        
        // Easily add a new version of the endpoint => WithName + "V2", HasApiVersion(2.0)
        app.MapGet(ApiEndpoints.Movies.GetAll, async (
                [AsParameters] GetAllMoviesRequest request,
                IMovieService movieService,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                var userId = context.GetUserId();
                var options = request.MapToOptions(userId);

                var movies = await movieService.GetAllAsync(options, cancellationToken);
                var count = await movieService.CountAsync(options.Title, options.YearOfRelease, cancellationToken);

                var response = movies.MapToResponse(request, count);

                return TypedResults.Ok(response);
            })
            .WithName(Name + "V2")
            .Produces<MoviesResponse>(StatusCodes.Status200OK)
            .WithApiVersionSet(ApiVersioning.VersionSet)
            .HasApiVersion(2.0)
            .CacheOutput("MovieCache");

        return app;
    }
}
