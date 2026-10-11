using ClineTestCore.Models;
using ClineTestCore.Repositories;
using MediatR;

namespace ClineTestApp.Queries.GetGenres;

public sealed class GetGenresQueryHandler : IRequestHandler<GetGenresQuery, IEnumerable<Genre>>
{
    private readonly IGenreRepository _genreRepository;

    public GetGenresQueryHandler(IGenreRepository genreRepository)
    {
        _genreRepository = genreRepository;
    }

    public Task<IEnumerable<Genre>> Handle(GetGenresQuery request, CancellationToken cancellationToken)
    {
        return _genreRepository.GetAllAsync(cancellationToken);
    }
}
