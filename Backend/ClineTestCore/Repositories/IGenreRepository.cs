using ClineTestCore.Models;

namespace ClineTestCore.Repositories;

public interface IGenreRepository
{
    Task<IEnumerable<Genre>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Genre?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Genre>> GetByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default);

    void Add(Genre genre);
}
