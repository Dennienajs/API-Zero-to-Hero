using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Controllers;

[ApiVersion(1.0)]
public class RatingsController : AuthorizeControllerBase
{
    private readonly IRatingService _ratingService;
    
    public RatingsController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }
    
    [HttpPut(ApiEndpoints.Movies.Rate)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Rate(
        [FromRoute] Guid id, 
        [FromBody] RateMovieRequest request, 
        CancellationToken cancellationToken)
    {
        return await _ratingService.RateMovieAsync(
                id, UserId, request.Rating, cancellationToken) 
                ? NoContent() 
                : NotFound();
    }
    
    [HttpDelete(ApiEndpoints.Movies.DeleteRating)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRating(
        [FromRoute] Guid id, 
        CancellationToken cancellationToken)
    {
        return await _ratingService.DeleteRatingAsync(id, UserId, cancellationToken) 
            ? NoContent() 
            : NotFound();
    }
    
    [HttpGet(ApiEndpoints.Ratings.GetUserRatings)]
    [ProducesResponseType(typeof(MovieRatingsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserRatings(CancellationToken cancellationToken)
    {
        return Ok((await _ratingService.GetUserRatingsAsync(UserId, cancellationToken)).MapToResponse());
    }
    
}
