using ClineTestCore.Repositories;
using ClineTestCore.Search;
using ClineTestCore.Storage;
using MediatR;

namespace ClineTestApp.Commands.UpdateMoviePoster;

public sealed class UpdateMoviePosterCommandHandler : IRequestHandler<UpdateMoviePosterCommand, string?>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorage _fileStorage;
    private readonly IMovieSearchService _movieSearchService;

    public UpdateMoviePosterCommandHandler(
        IMovieRepository movieRepository,
        IUnitOfWork unitOfWork,
        IFileStorage fileStorage,
        IMovieSearchService movieSearchService
    )
    {
        _movieRepository = movieRepository;
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _movieSearchService = movieSearchService;
    }

    public async Task<string?> Handle(UpdateMoviePosterCommand request, CancellationToken cancellationToken)
    {
        var movie = await _movieRepository.GetByIdAsync(request.MovieId, cancellationToken);

        if (movie is null)
        {
            return null;
        }

        var key = BuildKey(request);
        var posterUrl = _fileStorage.BuildUrl(key);

        movie.PosterUrl = posterUrl;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _fileStorage.UploadAsync(key, request.Content, request.ContentType, cancellationToken);
        await _movieSearchService.IndexAsync(movie, cancellationToken);

        return posterUrl;
    }

    private static string BuildKey(UpdateMoviePosterCommand request)
    {
        var extension = Path.GetExtension(request.FileName)?.ToLowerInvariant() ?? string.Empty;

        return $"posters/{request.MovieId:D}{extension}";
    }
}
