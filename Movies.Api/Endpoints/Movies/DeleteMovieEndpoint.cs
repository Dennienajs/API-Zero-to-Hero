using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Application.Services;

namespace Movies.Api.Endpoints.Movies;

public static class MapDeleteMovieEndpoint
{
    public const string Name = "DeleteMovie";

    public static IEndpointRouteBuilder MapDeleteMovie(this IEndpointRouteBuilder app)
    {
        app.MapDelete(ApiEndpoints.Movies.Delete, async (
                Guid id, 
                IMovieService movieService, 
                IOutputCacheStore outputCacheStore,
                CancellationToken cancellationToken) => 
            {
                var wasDeleted = await movieService.DeleteByIdAsync(id, cancellationToken);
        
                if (!wasDeleted)
                {
                    return Results.NotFound();
                }
        
                await outputCacheStore.EvictByTagAsync("movies", cancellationToken);
                return Results.NoContent();
            })
            .WithName(Name)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthConstants.AdminUserPolicyName);

        return app;
    }
}
