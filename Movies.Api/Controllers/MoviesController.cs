using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

[ApiVersion(1.0)]
public class MoviesController : ApiControllerBase
{
    private readonly IMovieService _movieService;
    
    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [Authorize(AuthConstants.TrustedMemberPolicyName)]
    [HttpPost(Endpoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request, CancellationToken cancellationToken)
    {
        var movie = request.MapToMovie();
        await _movieService.CreateAsync(movie, cancellationToken);

        return CreatedAtAction(nameof(Get), new { idOrSlug = movie.Id }, movie.MapToResponse());
    }

    [HttpGet(Endpoints.Movies.Get)]
    public async Task<IActionResult> Get([FromRoute] string idOrSlug, CancellationToken cancellationToken)
    {
        var movie = Guid.TryParse(idOrSlug, out var id) 
            ? await _movieService.GetByIdAsync(id, UserId, cancellationToken) 
            : await _movieService.GetBySlugAsync(idOrSlug, UserId, cancellationToken);
        
        return movie is not null
            ? Ok(movie.MapToResponse()) 
            : NotFound();
    }
    
    [HttpGet(Endpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll([FromQuery] GetAllMoviesRequest request, CancellationToken cancellationToken)
    {
        var options = request.MapToOptions(UserId);
        
        var movies = await _movieService.GetAllAsync(options, cancellationToken);
        var count = await _movieService.CountAsync(options.Title, options.YearOfRelease, cancellationToken);
        
        var response = movies.MapToResponse(request.Page, request.PageSize, count);
        
        return Ok(response);
    }

    [Authorize(AuthConstants.TrustedMemberPolicyName)]
    [HttpPut(Endpoints.Movies.Update)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id, 
        [FromBody] UpdateMovieRequest request, 
        CancellationToken cancellationToken)
    {
        var movie = request.MapToMovie(id);
        var updatedMovie = await _movieService.UpdateAsync(movie, UserId, cancellationToken);
            
        return updatedMovie is not null
            ? Ok(updatedMovie.MapToResponse()) 
            : NotFound();
    }
    
    [Authorize(AuthConstants.AdminUserPolicyName)]
    [HttpDelete(Endpoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        return await _movieService.DeleteByIdAsync(id, cancellationToken) 
            ? NoContent() 
            : NotFound();
    }
}
