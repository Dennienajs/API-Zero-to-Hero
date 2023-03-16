using FluentValidation;
using Movies.Application.Models;

namespace Movies.Application.Validators;

public class GetAllMoviesOptionsValidator : AbstractValidator<GetAllMoviesOptions>
{
    public GetAllMoviesOptionsValidator()
    {
        RuleFor(x => x.YearOfRelease)
            .GreaterThan(1900)
            .LessThanOrEqualTo(DateTime.UtcNow.Year + 1);
        
        RuleFor(x => x.SortField)
            .Must(x => x is null || AcceptedSortFields.Contains(x, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"You can only sort by '{nameof(Movie.Title)}' or '{nameof(Movie.YearOfRelease)}'.");
    }
    
    private static readonly string[] AcceptedSortFields = {
        nameof(Movie.Title),
        nameof(Movie.YearOfRelease)
    };
}
