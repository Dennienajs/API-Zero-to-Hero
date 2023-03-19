using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Responses;

namespace Movies.Api.Endpoints.Ratings;

public static class GetUserRatingsEndpoint
{
    public const string Name = "GetUserRatings";

    public static IEndpointRouteBuilder MapGetUserRatings(this IEndpointRouteBuilder app)
    {
        app.MapGet(ApiEndpoints.Ratings.GetUserRatings, async (
                    IRatingService ratingService,
                    HttpContext context,
                    CancellationToken cancellationToken) => 
               TypedResults.Ok((await ratingService.GetUserRatingsAsync(
                       context.GetUserId()!.Value, 
                       cancellationToken))
                   .MapToResponse()))
            .WithName(Name)
            .Produces<MovieRatingsResponse>(StatusCodes.Status200OK)
            .RequireAuthorization();

        return app;
    }
}