using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Application.Services;

namespace Movies.Api.Endpoints.Ratings;

public static class DeleteRatingEndpoint
{
    public const string Name = "DeleteRating";

    public static IEndpointRouteBuilder MapDeleteRating(this IEndpointRouteBuilder app)
    {
        app.MapDelete(ApiEndpoints.Movies.DeleteRating, async (
                    [FromRoute] Guid id,
                    IRatingService ratingService,
                    HttpContext context,
                    CancellationToken cancellationToken) =>
                await ratingService.DeleteRatingAsync(id, context.GetUserId()!.Value, cancellationToken)
                    ? Results.NoContent()
                    : Results.NotFound())
            .WithName(Name)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        return app;
    }
}
