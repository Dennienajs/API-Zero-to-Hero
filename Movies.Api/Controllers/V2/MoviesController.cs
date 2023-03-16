using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers.V2;

public class MoviesController : V1.ApiControllerBase
{
    private readonly IMovieService _movieService;
    
    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpGet(Endpoints.V2.Movies.Get)]
    public async Task<IActionResult> Get([FromRoute] string idOrSlug, CancellationToken cancellationToken)
    {
        var movie = Guid.TryParse(idOrSlug, out var id) 
            ? await _movieService.GetByIdAsync(id, UserId, cancellationToken) 
            : await _movieService.GetBySlugAsync(idOrSlug, UserId, cancellationToken);
        
        return movie is not null
            ? Ok(movie.MapToResponse()) 
            : NotFound();
    }
}
