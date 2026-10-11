using ClineTestCore.Models;
using ClineTestCore.Repositories;

namespace ClineTestApp.Genres;

internal static class MovieGenreResolver
{
    public static IReadOnlyList<string> NormalizeNames(IEnumerable<string> names)
    {
        return names.Select(name => name?.Trim() ?? string.Empty).Where(name => name.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    public static async Task<List<Genre>> ResolveAsync(
        IGenreRepository genreRepository,
        IEnumerable<string> names,
        CancellationToken cancellationToken
    )
    {
        var normalized = NormalizeNames(names);

        if (normalized.Count == 0)
        {
            return new List<Genre>();
        }

        var existing = await genreRepository.GetByNamesAsync(normalized, cancellationToken);
        var existingNames = new HashSet<string>(existing.Select(genre => genre.Name), StringComparer.OrdinalIgnoreCase);

        var genres = existing.ToList();
        genres.AddRange(normalized.Where(name => !existingNames.Contains(name)).Select(name => new Genre { Name = name }));

        return genres;
    }
}
