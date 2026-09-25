using ClineTestCore.Repositories;
using ClineTestCore.Storage;
using MediatR;

namespace ClineTestApp.Commands.UpdateMoviePoster;

public sealed class UpdateMoviePosterCommandHandler : IRequestHandler<UpdateMoviePosterCommand, string?>
{
    private readonly IMovieRepository _movieRepository;
    private readonly IFileStorage _fileStorage;

    public UpdateMoviePosterCommandHandler(IMovieRepository movieRepository, IFileStorage fileStorage)
    {
        _movieRepository = movieRepository;
        _fileStorage = fileStorage;
    }

    public async Task<string?> Handle(UpdateMoviePosterCommand request, CancellationToken cancellationToken)
    {
        var movie = await _movieRepository.GetByIdAsync(request.MovieId, cancellationToken);

        if (movie is null)
        {
            return null;
        }

        var posterUrl = await _fileStorage.UploadAsync(BuildKey(request), request.Content, request.ContentType, cancellationToken);

        await _movieRepository.UpdatePosterUrlAsync(movie.Id, posterUrl, cancellationToken);

        return posterUrl;
    }

    private static string BuildKey(UpdateMoviePosterCommand request)
    {
        var extension = Path.GetExtension(request.FileName)?.ToLowerInvariant() ?? string.Empty;

        return $"posters/{request.MovieId:D}{extension}";
    }
}
