using ClineTestApp.Commands.UpdateMoviePoster;
using ClineTestCore.Models;
using ClineTestCore.Repositories;
using ClineTestCore.Search;
using ClineTestCore.Storage;
using Moq;

namespace ClineTestAppUnitTests;

public class UpdateMoviePosterCommandHandlerTests
{
    private const string UploadedPosterUrl = "https://storage.example.com/posters/poster.jpg";

    private readonly Mock<IMovieRepository> _movieRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IFileStorage> _fileStorage = new();
    private readonly Mock<IMovieSearchService> _movieSearchService = new();
    private readonly UpdateMoviePosterCommandHandler _handler;

    public UpdateMoviePosterCommandHandlerTests()
    {
        _fileStorage.Setup(storage => storage.BuildUrl(It.IsAny<string>())).Returns(UploadedPosterUrl);
        _unitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _handler = new UpdateMoviePosterCommandHandler(_movieRepository.Object, _unitOfWork.Object, _fileStorage.Object, _movieSearchService.Object);
    }

    [Fact]
    public async Task Handle_CommitsTheGeneratedUrlAndUploadsThePoster()
    {
        var movie = CreateMovie();
        using var content = new MemoryStream([1, 2, 3]);
        var command = CreateCommand(movie.Id, content, "poster.jpg", "image/jpeg");
        _movieRepository.Setup(repository => repository.GetByIdAsync(movie.Id, It.IsAny<CancellationToken>())).ReturnsAsync(movie);
        _fileStorage
            .Setup(storage => storage.UploadAsync($"posters/{movie.Id:D}.jpg", content, "image/jpeg", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(UploadedPosterUrl, result);
        Assert.Equal(UploadedPosterUrl, movie.PosterUrl);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ReindexesMovieWithTheNewPosterUrl()
    {
        var movie = CreateMovie();
        using var content = new MemoryStream([1, 2, 3]);
        var command = CreateCommand(movie.Id, content, "poster.jpg", "image/jpeg");
        _movieRepository.Setup(repository => repository.GetByIdAsync(movie.Id, It.IsAny<CancellationToken>())).ReturnsAsync(movie);

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(UploadedPosterUrl, movie.PosterUrl);
        _movieSearchService.Verify(service => service.IndexAsync(movie, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_CommitsBeforeCallingTheOtherServices()
    {
        var movie = CreateMovie();
        using var content = new MemoryStream([1, 2, 3]);
        var command = CreateCommand(movie.Id, content, "poster.jpg", "image/jpeg");
        var calls = new List<string>();
        _movieRepository.Setup(repository => repository.GetByIdAsync(movie.Id, It.IsAny<CancellationToken>())).ReturnsAsync(movie);
        _fileStorage
            .Setup(storage => storage.UploadAsync(It.IsAny<string>(), content, "image/jpeg", It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("upload"))
            .Returns(Task.CompletedTask);
        _unitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>())).Callback(() => calls.Add("save")).ReturnsAsync(1);
        _movieSearchService
            .Setup(service => service.IndexAsync(It.IsAny<Movie>(), It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("index"))
            .Returns(Task.CompletedTask);

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(["save", "upload", "index"], calls);
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
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _movieSearchService.Verify(service => service.IndexAsync(It.IsAny<Movie>(), It.IsAny<CancellationToken>()), Times.Never);
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
            .Returns(Task.CompletedTask);

        await _handler.Handle(command, CancellationToken.None);

        Assert.Equal(expectedKey, uploadedKey);
    }

    [Fact]
    public async Task Handle_ForwardsCancellationTokenToRepositoryUnitOfWorkAndStorage()
    {
        var movie = CreateMovie();
        using var content = new MemoryStream([1, 2, 3]);
        using var cancellationTokenSource = new CancellationTokenSource();
        var command = CreateCommand(movie.Id, content, "poster.jpg", "image/jpeg");
        _movieRepository.Setup(repository => repository.GetByIdAsync(movie.Id, cancellationTokenSource.Token)).ReturnsAsync(movie);
        _fileStorage
            .Setup(storage => storage.UploadAsync(It.IsAny<string>(), content, "image/jpeg", cancellationTokenSource.Token))
            .Returns(Task.CompletedTask);
        _unitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync(cancellationTokenSource.Token)).ReturnsAsync(1);

        await _handler.Handle(command, cancellationTokenSource.Token);

        _movieRepository.Verify(repository => repository.GetByIdAsync(movie.Id, cancellationTokenSource.Token), Times.Once);
        _fileStorage.Verify(storage => storage.UploadAsync(It.IsAny<string>(), content, "image/jpeg", cancellationTokenSource.Token), Times.Once);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(cancellationTokenSource.Token), Times.Once);
        _movieSearchService.Verify(service => service.IndexAsync(movie, cancellationTokenSource.Token), Times.Once);
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
