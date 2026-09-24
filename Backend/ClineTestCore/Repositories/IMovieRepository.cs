using ClineTestCore.Models;

namespace ClineTestCore.Repositories;

public interface IMovieRepository
{
    Task<IEnumerable<Movie>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Movie movie, CancellationToken cancellationToken = default);
}
