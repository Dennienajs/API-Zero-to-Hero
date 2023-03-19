using System.Text.RegularExpressions;

namespace Movies.Application.Models;

public partial class Movie
{
    public required Guid Id { get; init; }
    public required string Title { get; set; }
    public string Slug => GenerateSlug();
    public required int YearOfRelease { get; set; }
    public List<string> Genres { get; init; } = new();
    
    public float? AverageRating { get; set; }
    public int? UserRating { get; set; }

    private string GenerateSlug()
    {
        var slug = SlugRegex()
            .Replace(Title, string.Empty)
            .ToLower()
            .Replace(" ", "-");
        
        return $"{slug}-{YearOfRelease}";
    }

    [GeneratedRegex("[^a-zA-Z0-9\\s-]", RegexOptions.NonBacktracking, 5)]
    private static partial Regex SlugRegex();
}