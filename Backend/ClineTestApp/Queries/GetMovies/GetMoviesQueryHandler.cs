using ClineTestCore.Models;
using ClineTestCore.Repositories;
using MediatR;

namespace ClineTestApp.Queries.GetMovies;

public sealed class GetMoviesQueryHandler : IRequestHandler<GetMoviesQuery, IEnumerable<Movie>>
{
    private readonly IMovieRepository _movieRepository;

    public GetMoviesQueryHandler(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public Task<IEnumerable<Movie>> Handle(GetMoviesQuery request, CancellationToken cancellationToken)
    {
        return _movieRepository.GetAllAsync(cancellationToken);
    }
}
