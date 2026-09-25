

namespace ClineTestCore.Models;

public class Movie
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public int Year { get; set; }
    public required string Description { get; set; }
    public required string Director { get; set; }
    public required string Overview { get; set; }
    public required string OriginalLanguage { get; set; }
    public required string PosterUrl { get; set; }
}

