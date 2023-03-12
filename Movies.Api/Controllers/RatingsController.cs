using Microsoft.AspNetCore.Mvc;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

public class RatingsController : AuthorizeControllerBase
{
    private readonly IRatingService _ratingService;
    
    public RatingsController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }
    
    [HttpPut(Endpoints.Movies.Rate)]
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
    
    [HttpDelete(Endpoints.Movies.DeleteRating)]
    public async Task<IActionResult> DeleteRating(
        [FromRoute] Guid id, 
        CancellationToken cancellationToken)
    {
        return await _ratingService.DeleteRatingAsync(id, UserId, cancellationToken) 
            ? NoContent() 
            : NotFound();
    }
    
    [HttpGet(Endpoints.Ratings.GetUserRatings)]
    public async Task<IActionResult> GetUserRatings(CancellationToken cancellationToken)
    {
        return Ok((await _ratingService.GetUserRatingsAsync(UserId, cancellationToken)).MapToResponse());
    }
    
}
