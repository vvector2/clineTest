using ClineTestCore.Models;

namespace ClineTestCore.Search;

public interface IMovieSearchService
{
    Task IndexAsync(Movie movie, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Movie>> SearchAsync(string query, CancellationToken cancellationToken = default);
}
