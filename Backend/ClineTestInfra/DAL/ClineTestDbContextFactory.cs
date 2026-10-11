using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClineTestInfra.DAL;

public sealed class ClineTestDbContextFactory : IDesignTimeDbContextFactory<ClineTestDbContext>
{
    public ClineTestDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ClineTestDbContext>();
        var serverVersion = new MySqlServerVersion(new Version(8, 4, 0));

        optionsBuilder.UseMySql("Server=localhost;Database=clinetest;User=clinetest;Password=clinetest;", serverVersion);

        return new ClineTestDbContext(optionsBuilder.Options);
    }
}
