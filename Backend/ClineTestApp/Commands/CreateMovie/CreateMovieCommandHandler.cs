using ClineTestCore.Models;
using ClineTestCore.Repositories;
using ClineTestCore.Search;
using MediatR;

namespace ClineTestApp.Commands.CreateMovie;

public sealed class CreateMovieCommandHandler : IRequestHandler<CreateMovieCommand, Guid>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMovieSearchService _movieSearchService;

    public CreateMovieCommandHandler(IMovieRepository movieRepository, IUnitOfWork unitOfWork, IMovieSearchService movieSearchService)
    {
        _movieRepository = movieRepository;
        _unitOfWork = unitOfWork;
        _movieSearchService = movieSearchService;
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
            PosterUrl = request.PosterUrl,
        };

        _movieRepository.Add(movie);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _movieSearchService.IndexAsync(movie, cancellationToken);

        return movie.Id;
    }
}
