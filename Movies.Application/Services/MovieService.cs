using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Application.Validators;

namespace Movies.Application.Services;

public interface IMovieService
{
    Task<bool> CreateAsync(Movie movie);
    Task<Movie?> GetByIdAsync(Guid id);
    Task<Movie?> GetBySlugAsync(string slug);
    Task<IEnumerable<Movie>> GetAllAsync();
    Task<Movie?> UpdateAsync(Movie movie);
    Task<bool> DeleteByIdAsync(Guid id);
}

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;
    private readonly IValidator<Movie> _validator;

    public MovieService(
        IMovieRepository movieRepository, 
        IValidator<Movie> validator)
    {
        _movieRepository = movieRepository;
        _validator = validator;
    }
    
    public async Task<bool> CreateAsync(Movie movie)
    {
        await _validator.ValidateAndThrowAsync(movie);
        return await _movieRepository.CreateAsync(movie);
    }
    
    public async Task<Movie?> GetByIdAsync(Guid id)
    {
        return await _movieRepository.GetByIdAsync(id);
    }
    
    public async Task<Movie?> GetBySlugAsync(string slug)
    {
        return await _movieRepository.GetBySlugAsync(slug);
    }
    
    public async Task<IEnumerable<Movie>> GetAllAsync()
    {
        return await _movieRepository.GetAllAsync();
    }
    
    public async Task<Movie?> UpdateAsync(Movie movie)
    {
        await _validator.ValidateAndThrowAsync(movie);
        return await _movieRepository.ExistsByIdAsync(movie.Id)
            && await _movieRepository.UpdateAsync(movie) 
                ? movie 
                : null;
    }
    
    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        return await _movieRepository.DeleteByIdAsync(id);
    }
}
