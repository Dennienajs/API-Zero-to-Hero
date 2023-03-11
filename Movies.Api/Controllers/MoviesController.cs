using Microsoft.AspNetCore.Mvc;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

[ApiController]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;
    
    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    [HttpPost(Endpoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request)
    {
        var movie = request.MapToMovie();
        await _movieService.CreateAsync(movie);

        return CreatedAtAction(nameof(Get), new { idOrSlug = movie.Id }, movie.MapToResponse());
    }

    [HttpGet(Endpoints.Movies.Get)]
    public async Task<IActionResult> Get([FromRoute] string idOrSlug)
    {
        var movie = Guid.TryParse(idOrSlug, out var id) 
            ? await _movieService.GetByIdAsync(id) 
            : await _movieService.GetBySlugAsync(idOrSlug);
        
        return movie is not null
            ? Ok(movie.MapToResponse()) 
            : NotFound();
    }
    
    [HttpGet(Endpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll()
    {
        return Ok((await _movieService.GetAllAsync()).MapToResponse());
    }

    [HttpPut(Endpoints.Movies.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest request)
    {
        var movie = request.MapToMovie(id);
        var updatedMovie = await _movieService.UpdateAsync(movie);
            
        return updatedMovie is not null
            ? Ok(updatedMovie.MapToResponse()) 
            : NotFound();
    }
    
    [HttpDelete(Endpoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        return await _movieService.DeleteByIdAsync(id) 
            ? NoContent() 
            : NotFound();
    }
}
