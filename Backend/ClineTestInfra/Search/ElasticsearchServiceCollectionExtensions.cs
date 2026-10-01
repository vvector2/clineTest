using ClineTestCore.Search;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClineTestInfra.Search;

public static class ElasticsearchServiceCollectionExtensions
{
    public static IServiceCollection AddMoviesSearch(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var options = ResolveOptions(configuration);

        services.AddSingleton(options);
        services.AddSingleton(_ => new ElasticsearchClient(new ElasticsearchClientSettings(new Uri(options.Url))));
        services.AddSingleton<IMovieSearchService, ElasticsearchMovieSearchService>();

        return services;
    }

    private static ElasticsearchOptions ResolveOptions(IConfiguration configuration)
    {
        var section = configuration.GetSection(ElasticsearchOptions.SectionName);

        var options = new ElasticsearchOptions
        {
            Url = ResolveValue(section[nameof(ElasticsearchOptions.Url)], "CLINETEST_ELASTICSEARCH_URL") ?? string.Empty,
            IndexName = ResolveValue(section[nameof(ElasticsearchOptions.IndexName)], "CLINETEST_ELASTICSEARCH_INDEX") ?? string.Empty,
        };

        if (string.IsNullOrWhiteSpace(options.Url))
        {
            throw new InvalidOperationException(
                "Elasticsearch url is not configured. Set the Elasticsearch__Url configuration value or the CLINETEST_ELASTICSEARCH_URL environment variable."
            );
        }

        if (string.IsNullOrWhiteSpace(options.IndexName))
        {
            throw new InvalidOperationException(
                "Elasticsearch index name is not configured. Set the Elasticsearch__IndexName configuration value or the CLINETEST_ELASTICSEARCH_INDEX environment variable."
            );
        }

        return options;
    }

    private static string? ResolveValue(string? value, string environmentVariable)
    {
        return string.IsNullOrWhiteSpace(value) ? Environment.GetEnvironmentVariable(environmentVariable) : value;
    }
}
