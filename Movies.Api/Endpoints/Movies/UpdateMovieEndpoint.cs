using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Endpoints.Movies;

public static class UpdateMovieEndpoint
{
    public const string Name = "UpdateMovie";

    public static IEndpointRouteBuilder MapUpdateMovie(this IEndpointRouteBuilder app)
    {
        app.MapPut(ApiEndpoints.Movies.Update, async (
                [FromRoute] Guid id,
                [FromBody] UpdateMovieRequest request,
                IMovieService movieService,
                IOutputCacheStore outputCacheStore,
                HttpContext context,
                CancellationToken cancellationToken) =>
            {
                var userId = context.GetUserId();
                var movie = request.MapToMovie(id);

                var updatedMovie = await movieService.UpdateAsync(movie, userId, cancellationToken);

                if (updatedMovie is null)
                {
                    return Results.NotFound();
                }

                await outputCacheStore.EvictByTagAsync("movies", cancellationToken);
                return TypedResults.Ok(updatedMovie.MapToResponse());
            })
            .WithName(Name)
            .Produces<MovieResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces<ValidationFailureResponse>(StatusCodes.Status400BadRequest)
            .RequireAuthorization(AuthConstants.TrustedMemberPolicyName);

        return app;
    }
}