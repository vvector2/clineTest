using ClineTestCore.Models;
using ClineTestCore.Repositories;
using MediatR;

namespace ClineTestApp.Queries.GetGenreByName;

public sealed class GetGenreByNameQueryHandler : IRequestHandler<GetGenreByNameQuery, Genre?>
{
    private readonly IGenreRepository _genreRepository;

    public GetGenreByNameQueryHandler(IGenreRepository genreRepository)
    {
        _genreRepository = genreRepository;
    }

    public Task<Genre?> Handle(GetGenreByNameQuery request, CancellationToken cancellationToken)
    {
        return _genreRepository.GetByNameAsync(request.Name, cancellationToken);
    }
}
