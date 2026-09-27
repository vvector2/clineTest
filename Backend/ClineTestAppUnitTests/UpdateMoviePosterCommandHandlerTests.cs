using ClineTestApp.Commands.UpdateMoviePoster;
using ClineTestCore.Models;
using ClineTestCore.Repositories;
using ClineTestCore.Storage;
using Moq;

namespace ClineTestAppUnitTests;

public class UpdateMoviePosterCommandHandlerTests
{
    private const string UploadedPosterUrl = "https://storage.example.com/posters/poster.jpg";

    private readonly Mock<IMovieRepository> _movieRepository = new();
    private readonly Mock<IFileStorage> _fileStorage = new();
    private readonly UpdateMoviePosterCommandHandler _handler;

    public UpdateMoviePosterCommandHandlerTests()
    {
        _handler = new UpdateMoviePosterCommandHandler(_movieRepository.Object, _fileStorage.Object);
    }

    [Fact]
    public async Task Handle_UploadsPosterAndPersistsReturnedUrl()
    {
        var movie = CreateMovie();
        using var content = new MemoryStream([1, 2, 3]);
        var command = CreateCommand(movie.Id, content, "poster.jpg", "image/jpeg");
        _movieRepository.Setup(repository => repository.GetByIdAsync(movie.Id, It.IsAny<CancellationToken>())).ReturnsAsync(movie);
        _fileStorage
            .Setup(storage => storage.UploadAsync($"posters/{movie.Id:D}.jpg", content, "image/jpeg", It.IsAny<CancellationToken>()))
            .ReturnsAsync(UploadedPosterUrl);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(UploadedPosterUrl, result);
        _movieRepository.Verify(repository => repository.UpdatePosterUrlAsync(movie.Id, UploadedPosterUrl, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsNullAndSkipsStorageWhenMovieDoesNotExist()
    {
        var movieId = Guid.NewGuid();
        using var content = new MemoryStream([1, 2, 3]);
        var command = CreateCommand(movieId, content, "poster.jpg", "image/jpeg");
        _movieRepository.Setup(repository => repository.GetByIdAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync((Movie?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Null(result);
        _fileStorage.Verify(
            storage => storage.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _movieRepository.Verify(
            repository => repository.UpdatePosterUrlAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Theory]
    [InlineData("poster.JPG", ".jpg")]
    [InlineData("poster.PostEr.PNG", ".png")]
    [InlineData("photo.webp", ".webp")]
    [InlineData("poster", "")]
    [InlineData("poster.", "")]
    public async Task Handle_BuildsLowerCasedStorageKeyFromPosterFileName(string fileName, string expectedExtension)
    {
        var movie = CreateMovie();
        using var content = new MemoryStream([1, 2, 3]);
        var command = CreateCommand(movie.Id, content, fileName, "image/jpeg");
        var expectedKey = $"posters/{movie.Id:D}{expectedExtension}";
        _movieRepository.Setup(repository => repository.GetByIdAsync(movie.Id, It.IsAny<CancellationToken>())).ReturnsAsync(movie);
        string? uploadedKey = null;
        _fileStorage
            .Setup(storage => storage.UploadAsync(It.IsAny<string>(), content, "image/jpeg", It.IsAny<CancellationToken>()))
            .Callback<string, Stream, string, CancellationToken>((key, _, _, _) => uploadedKey = key)
            .ReturnsAsync(UploadedPosterUrl);

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(expectedKey, uploadedKey);
    }

    [Fact]
    public async Task Handle_ForwardsCancellationTokenToRepositoryAndStorage()
    {
        var movie = CreateMovie();
        using var content = new MemoryStream([1, 2, 3]);
        using var cancellationTokenSource = new CancellationTokenSource();
        var command = CreateCommand(movie.Id, content, "poster.jpg", "image/jpeg");
        _movieRepository.Setup(repository => repository.GetByIdAsync(movie.Id, cancellationTokenSource.Token)).ReturnsAsync(movie);
        _fileStorage
            .Setup(storage => storage.UploadAsync(It.IsAny<string>(), content, "image/jpeg", cancellationTokenSource.Token))
            .ReturnsAsync(UploadedPosterUrl);

        await _handler.Handle(command, cancellationTokenSource.Token);

        _movieRepository.Verify(repository => repository.GetByIdAsync(movie.Id, cancellationTokenSource.Token), Times.Once);
        _fileStorage.Verify(storage => storage.UploadAsync(It.IsAny<string>(), content, "image/jpeg", cancellationTokenSource.Token), Times.Once);
        _movieRepository.Verify(
            repository => repository.UpdatePosterUrlAsync(movie.Id, UploadedPosterUrl, cancellationTokenSource.Token),
            Times.Once
        );
    }

    private static Movie CreateMovie()
    {
        return new Movie
        {
            Id = Guid.NewGuid(),
            Title = "Inception",
            Year = 2010,
            Description = "A thief who steals corporate secrets.",
            Director = "Christopher Nolan",
            Overview = "Dreams within dreams.",
            OriginalLanguage = "en",
            PosterUrl = "https://example.com/posters/inception.jpg",
        };
    }

    private static UpdateMoviePosterCommand CreateCommand(Guid movieId, Stream content, string fileName, string contentType)
    {
        return new UpdateMoviePosterCommand
        {
            MovieId = movieId,
            Content = content,
            FileName = fileName,
            ContentType = contentType,
        };
    }
}
