using ClineTestCore.Models;
using ClineTestCore.Repositories;
using ClineTestInfra.DAL;
using Microsoft.EntityFrameworkCore;

namespace ClineTestInfra.DAL.Repositories;

public class GenreRepository : IGenreRepository
{
    private readonly ClineTestDbContext _dbContext;

    public GenreRepository(ClineTestDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Genre>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Genres.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Genre?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Genres.FirstOrDefaultAsync(genre => genre.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Genre>> GetByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default)
    {
        var distinctNames = names.Distinct().ToArray();

        if (distinctNames.Length == 0)
        {
            return [];
        }

        return await _dbContext.Genres.Where(genre => distinctNames.Contains(genre.Name)).ToListAsync(cancellationToken);
    }

    public void Add(Genre genre)
    {
        _dbContext.Genres.Add(genre);
    }
}
