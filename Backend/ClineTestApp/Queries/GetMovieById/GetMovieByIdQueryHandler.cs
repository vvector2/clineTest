using ClineTestCore.Models;
using ClineTestCore.Repositories;
using MediatR;

namespace ClineTestApp.Queries.GetMovieById;

public sealed class GetMovieByIdQueryHandler : IRequestHandler<GetMovieByIdQuery, Movie?>
{
    private readonly IMovieRepository _movieRepository;

    public GetMovieByIdQueryHandler(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public Task<Movie?> Handle(GetMovieByIdQuery request, CancellationToken cancellationToken)
    {
        return _movieRepository.GetByIdAsync(request.Id, cancellationToken);
    }
}
