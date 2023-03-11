using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Validators;

public class MovieValidator : AbstractValidator<Movie>
{
    private readonly IMovieRepository _movieRepository;
    
    public MovieValidator(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
        
        RuleFor(x=> x.Id)
            .NotEmpty();

        RuleFor(x => x.Title)
            .NotEmpty();

        RuleFor(x => x.YearOfRelease)
            .GreaterThan(1900)
            .LessThanOrEqualTo(DateTime.UtcNow.Year + 1);

        RuleFor(x => x.Genres)
            .NotEmpty()
            .ForEach(x => x.NotEmpty());

        RuleFor(x => x.Slug)
            .MustAsync(ValidateSlug)
            .WithMessage("Movie already exists");
    }
    
    private async Task<bool> ValidateSlug(
        Movie movie, 
        string slug,
        CancellationToken cancellationToken)
    {
        var existingMovie = await _movieRepository.GetBySlugAsync(slug);
        
        return existingMovie is null || existingMovie.Id == movie.Id;
    }
}
