using ClineTestCore.Models;

namespace ClientTestApi.Models;

public sealed record MovieResponse(
    Guid Id,
    string Title,
    int Year,
    string Description,
    string Director,
    string Overview,
    string OriginalLanguage,
    string PosterUrl,
    IReadOnlyList<string> Genres
)
{
    public static MovieResponse FromMovie(Movie movie)
    {
        return new MovieResponse(
            movie.Id,
            movie.Title,
            movie.Year,
            movie.Description,
            movie.Director,
            movie.Overview,
            movie.OriginalLanguage,
            movie.PosterUrl,
            movie.Genres.Select(genre => genre.Name).ToList()
        );
    }
}
