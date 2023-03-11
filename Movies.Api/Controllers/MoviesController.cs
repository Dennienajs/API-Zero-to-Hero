using Microsoft.AspNetCore.Mvc;
using Movies.Api.Mapping;
using Movies.Application.Repositories;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

[ApiController]
public class MoviesController : ControllerBase
{
    private readonly IMovieRepository _movieRepository;
    
    public MoviesController(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }
    
    [HttpPost(Endpoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request)
    {
        var movie = request.MapToMovie();
        await _movieRepository.CreateAsync(movie);

        return CreatedAtAction(nameof(Get), new { id = movie.Id }, movie.MapToResponse());
    }

    [HttpGet(Endpoints.Movies.Get)]
    public async Task<IActionResult> Get([FromRoute] Guid id)
    {
        return await _movieRepository.GetByIdAsync(id) is {} movie
            ? Ok(movie.MapToResponse()) 
            : NotFound();
    }
    
    [HttpGet(Endpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll()
    {
        return Ok((await _movieRepository.GetAllAsync()).MapToResponse());
    }

    [HttpPut(Endpoints.Movies.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest request)
    {
        var movie = request.MapToMovie(id);
        
        return await _movieRepository.UpdateAsync(movie) 
            ? Ok(movie.MapToResponse()) 
            : NotFound();
    }
    
    [HttpDelete(Endpoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        return await _movieRepository.DeleteByIdAsync(id) 
            ? NoContent() 
            : NotFound();
    }
}
