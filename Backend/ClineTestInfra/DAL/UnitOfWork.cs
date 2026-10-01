using ClineTestCore.Repositories;

namespace ClineTestInfra.DAL;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ClineTestDbContext _dbContext;

    public UnitOfWork(ClineTestDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
