using ClineTestCore.Models;
using ClineTestCore.Search;
using MediatR;

namespace ClineTestApp.Queries.SearchMovies;

public sealed class SearchMoviesQueryHandler : IRequestHandler<SearchMoviesQuery, IEnumerable<Movie>>
{
    private readonly IMovieSearchService _movieSearchService;

    public SearchMoviesQueryHandler(IMovieSearchService movieSearchService)
    {
        _movieSearchService = movieSearchService;
    }

    public async Task<IEnumerable<Movie>> Handle(SearchMoviesQuery request, CancellationToken cancellationToken)
    {
        return await _movieSearchService.SearchAsync(request.Query, cancellationToken);
    }
}
