using ClineTestCore.Repositories;
using ClineTestInfra.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClineTestInfra.DAL;

public static class DalServiceCollectionExtensions
{
    public static IServiceCollection AddDal(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = ResolveConnectionString(configuration);
        var serverVersion = ServerVersion.AutoDetect(connectionString);

        services.AddDbContext<ClineTestDbContext>(options => options.UseMySql(connectionString, serverVersion));
        services.AddScoped<IMovieRepository, MovieRepository>();

        return services;
    }

    private static string ResolveConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = Environment.GetEnvironmentVariable("CLINETEST_DB_CONNECTION");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "MySQL connection string is not configured. Set the ConnectionStrings__DefaultConnection or CLINETEST_DB_CONNECTION environment variable.");
        }

        return connectionString;
    }
}
