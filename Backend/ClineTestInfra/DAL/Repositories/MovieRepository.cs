using ClineTestCore.Models;
using ClineTestCore.Repositories;
using ClineTestInfra.DAL;
using Microsoft.EntityFrameworkCore;

namespace ClineTestInfra.DAL.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly ClineTestDbContext _dbContext;

    public MovieRepository(ClineTestDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Movies.Include(m => m.Genres).AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Movie?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Movies.Include(m => m.Genres).FirstOrDefaultAsync(movie => movie.Id == id, cancellationToken);
    }

    public void Add(Movie movie)
    {
        _dbContext.Movies.Add(movie);
    }
}
