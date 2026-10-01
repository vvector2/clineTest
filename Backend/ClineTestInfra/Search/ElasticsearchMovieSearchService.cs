using ClineTestCore.Models;
using ClineTestCore.Search;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace ClineTestInfra.Search;

public sealed class ElasticsearchMovieSearchService : IMovieSearchService
{
    private const int MaxResults = 50;

    private static readonly string[] SearchableFields = ["title", "director", "description", "overview"];

    private readonly ElasticsearchClient _client;
    private readonly ElasticsearchOptions _options;

    public ElasticsearchMovieSearchService(ElasticsearchClient client, ElasticsearchOptions options)
    {
        _client = client;
        _options = options;
    }

    public async Task IndexAsync(Movie movie, CancellationToken cancellationToken = default)
    {
        var document = MovieSearchDocument.FromMovie(movie);

        var response = await _client.IndexAsync(
            document,
            request => request.Index(_options.IndexName).Id(movie.Id.ToString("D")).Refresh(Refresh.WaitFor),
            cancellationToken
        );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Could not index the movie '{movie.Id:D}'. {response.DebugInformation}");
        }
    }

    public async Task<IReadOnlyCollection<Movie>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var existsResponse = await _client.Indices.ExistsAsync(_options.IndexName, cancellationToken);

        if (!existsResponse.Exists)
        {
            return [];
        }

        var response = await _client.SearchAsync<MovieSearchDocument>(
            search =>
                search
                    .Index(_options.IndexName)
                    .Size(MaxResults)
                    .Query(searchQuery =>
                        searchQuery.MultiMatch(match =>
                            match.Query(query).Fields(Fields.FromStrings(SearchableFields)).Fuzziness(new Fuzziness("AUTO"))
                        )
                    ),
            cancellationToken
        );

        if (!response.IsValidResponse)
        {
            throw new InvalidOperationException($"Could not search for movies. {response.DebugInformation}");
        }

        return response.Documents.Select(document => document.ToMovie()).ToArray();
    }
}
