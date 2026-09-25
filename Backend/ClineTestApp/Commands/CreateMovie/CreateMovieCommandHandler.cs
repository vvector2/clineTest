using ClineTestCore.Models;
using ClineTestCore.Repositories;
using MediatR;

namespace ClineTestApp.Commands.CreateMovie;

public sealed class CreateMovieCommandHandler : IRequestHandler<CreateMovieCommand, Guid>
{
    private readonly IMovieRepository _movieRepository;

    public CreateMovieCommandHandler(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<Guid> Handle(CreateMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = new Movie
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Year = request.Year,
            Description = request.Description,
            Director = request.Director,
            Overview = request.Overview,
            OriginalLanguage = request.OriginalLanguage,
            PosterUrl = request.PosterUrl
        };

        await _movieRepository.AddAsync(movie, cancellationToken);

        return movie.Id;
    }
}
