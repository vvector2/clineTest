using System.Text.Json.Serialization;
using ClineTestCore.Models;

namespace ClineTestInfra.Search;

internal sealed class MovieSearchDocument
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("year")]
    public int Year { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("director")]
    public string Director { get; set; } = string.Empty;

    [JsonPropertyName("overview")]
    public string Overview { get; set; } = string.Empty;

    [JsonPropertyName("originalLanguage")]
    public string OriginalLanguage { get; set; } = string.Empty;

    [JsonPropertyName("posterUrl")]
    public string PosterUrl { get; set; } = string.Empty;

    [JsonPropertyName("genres")]
    public List<string> Genres { get; set; } = new();

    public static MovieSearchDocument FromMovie(Movie movie)
    {
        return new MovieSearchDocument
        {
            Id = movie.Id,
            Title = movie.Title,
            Year = movie.Year,
            Description = movie.Description,
            Director = movie.Director,
            Overview = movie.Overview,
            OriginalLanguage = movie.OriginalLanguage,
            PosterUrl = movie.PosterUrl,
            Genres = movie.Genres.Select(genre => genre.Name).ToList(),
        };
    }

    public Movie ToMovie()
    {
        return new Movie
        {
            Id = Id,
            Title = Title,
            Year = Year,
            Description = Description,
            Director = Director,
            Overview = Overview,
            OriginalLanguage = OriginalLanguage,
            PosterUrl = PosterUrl,
            Genres = Genres.Select(name => new Genre { Name = name }).ToList(),
        };
    }
}
