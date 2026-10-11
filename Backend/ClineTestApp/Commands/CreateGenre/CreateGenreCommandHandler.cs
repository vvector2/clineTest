using ClineTestApp.Genres;
using ClineTestCore.Models;
using ClineTestCore.Repositories;
using MediatR;

namespace ClineTestApp.Commands.CreateGenre;

public sealed class CreateGenreCommandHandler : IRequestHandler<CreateGenreCommand, string>
{
    private readonly IGenreRepository _genreRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateGenreCommandHandler(IGenreRepository genreRepository, IUnitOfWork unitOfWork)
    {
        _genreRepository = genreRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> Handle(CreateGenreCommand request, CancellationToken cancellationToken)
    {
        var name = MovieGenreResolver.NormalizeNames([request.Name]).FirstOrDefault();

        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Genre name must not be empty.", nameof(request));
        }

        var existing = await _genreRepository.GetByNameAsync(name, cancellationToken);

        if (existing is not null)
        {
            return existing.Name;
        }

        var genre = new Genre { Name = name };

        _genreRepository.Add(genre);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return genre.Name;
    }
}
