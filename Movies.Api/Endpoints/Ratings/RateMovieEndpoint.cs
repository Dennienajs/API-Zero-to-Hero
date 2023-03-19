using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Endpoints.Ratings;


public static class RateMovieEndpoint
{
    public const string Name = "RateMovie";

    public static IEndpointRouteBuilder MapRateMovie(this IEndpointRouteBuilder app)
    {
        app.MapPut(ApiEndpoints.Movies.Rate, async (
                    [FromRoute] Guid id,
                    [FromBody] RateMovieRequest request, 
                    IRatingService ratingService,
                    HttpContext context,
                    CancellationToken cancellationToken) => 
                await ratingService.RateMovieAsync(
                    id, context.GetUserId()!.Value, request.Rating, cancellationToken) 
                    ? Results.NoContent() 
                    : Results.NotFound())
            .WithName(Name)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        return app;
    }
}