using ClineTestInfra.DAL;
using ClineTestInfra.Search;
using ClineTestInfra.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClineTestInfra;

public static class InfraServiceCollectionExtensions
{
    public static IServiceCollection AddInfra(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDal(configuration);
        services.AddS3Storage(configuration);
        services.AddMoviesSearch(configuration);

        return services;
    }
}
