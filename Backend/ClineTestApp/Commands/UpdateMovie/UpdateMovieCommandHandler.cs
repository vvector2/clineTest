using ClineTestApp.Genres;
using ClineTestCore.Models;
using ClineTestCore.Repositories;
using ClineTestCore.Search;
using MediatR;

namespace ClineTestApp.Commands.UpdateMovie;

public sealed class UpdateMovieCommandHandler : IRequestHandler<UpdateMovieCommand, Movie?>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IGenreRepository _genreRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMovieSearchService _movieSearchService;

    public UpdateMovieCommandHandler(
        IMovieRepository movieRepository,
        IGenreRepository genreRepository,
        IUnitOfWork unitOfWork,
        IMovieSearchService movieSearchService
    )
    {
        _movieRepository = movieRepository;
        _genreRepository = genreRepository;
        _unitOfWork = unitOfWork;
        _movieSearchService = movieSearchService;
    }

    public async Task<Movie?> Handle(UpdateMovieCommand request, CancellationToken cancellationToken)
    {
        var movie = await _movieRepository.GetByIdAsync(request.Id, cancellationToken);

        if (movie is null)
        {
            return null;
        }

        movie.Title = request.Title;
        movie.Year = request.Year;
        movie.Description = request.Description;
        movie.Director = request.Director;
        movie.Overview = request.Overview;
        movie.OriginalLanguage = request.OriginalLanguage;
        movie.PosterUrl = request.PosterUrl;

        var genres = await MovieGenreResolver.ResolveAsync(_genreRepository, request.Genres, cancellationToken);
        movie.Genres.Clear();
        foreach (var genre in genres)
        {
            movie.Genres.Add(genre);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _movieSearchService.IndexAsync(movie, cancellationToken);

        return movie;
    }
}
