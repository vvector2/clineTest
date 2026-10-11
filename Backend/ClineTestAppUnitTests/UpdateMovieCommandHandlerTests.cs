using ClineTestApp.Commands.UpdateMovie;
using ClineTestCore.Models;
using ClineTestCore.Repositories;
using ClineTestCore.Search;
using Moq;

namespace ClineTestAppUnitTests;

public class UpdateMovieCommandHandlerTests
{
    private readonly Mock<IMovieRepository> _movieRepository = new();
    private readonly Mock<IGenreRepository> _genreRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IMovieSearchService> _movieSearchService = new();
    private readonly UpdateMovieCommandHandler _handler;

    public UpdateMovieCommandHandlerTests()
    {
        _unitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _genreRepository
            .Setup(repository => repository.GetByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _handler = new UpdateMovieCommandHandler(_movieRepository.Object, _genreRepository.Object, _unitOfWork.Object, _movieSearchService.Object);
    }

    [Fact]
    public async Task Handle_OverwritesEveryMutableFieldAndReturnsTheMovie()
    {
        var movie = CreateMovie();
        _movieRepository.Setup(repository => repository.GetByIdAsync(movie.Id, It.IsAny<CancellationToken>())).ReturnsAsync(movie);
        var command = CreateCommand(movie.Id);

        var result = await _handler.Handle(command, CancellationToken.None);

        var updatedMovie = Assert.IsType<Movie>(result);
        Assert.Equal(movie.Id, updatedMovie.Id);
        Assert.Equal(command.Title, updatedMovie.Title);
        Assert.Equal(command.Year, updatedMovie.Year);
        Assert.Equal(command.Description, updatedMovie.Description);
        Assert.Equal(command.Director, updatedMovie.Director);
        Assert.Equal(command.Overview, updatedMovie.Overview);
        Assert.Equal(command.OriginalLanguage, updatedMovie.OriginalLanguage);
        Assert.Equal(command.PosterUrl, updatedMovie.PosterUrl);
    }

    [Fact]
    public async Task Handle_CommitsTheChanges()
    {
        var movie = CreateMovie();
        _movieRepository.Setup(repository => repository.GetByIdAsync(movie.Id, It.IsAny<CancellationToken>())).ReturnsAsync(movie);

        await _handler.Handle(CreateCommand(movie.Id), CancellationToken.None);

        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_CommitsBeforeIndexingTheUpdatedMovie()
    {
        var movie = CreateMovie();
        var calls = new List<string>();
        _movieRepository.Setup(repository => repository.GetByIdAsync(movie.Id, It.IsAny<CancellationToken>())).ReturnsAsync(movie);
        _unitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>())).Callback(() => calls.Add("save")).ReturnsAsync(1);
        _movieSearchService
            .Setup(service => service.IndexAsync(It.IsAny<Movie>(), It.IsAny<CancellationToken>()))
            .Callback(() => calls.Add("index"))
            .Returns(Task.CompletedTask);

        await _handler.Handle(CreateCommand(movie.Id), CancellationToken.None);

        Assert.Equal(["save", "index"], calls);
    }

    [Fact]
    public async Task Handle_ReindexesTheUpdatedMovie()
    {
        var movie = CreateMovie();
        var command = CreateCommand(movie.Id);
        Movie? indexedMovie = null;
        _movieRepository.Setup(repository => repository.GetByIdAsync(movie.Id, It.IsAny<CancellationToken>())).ReturnsAsync(movie);
        _movieSearchService
            .Setup(service => service.IndexAsync(It.IsAny<Movie>(), It.IsAny<CancellationToken>()))
            .Callback<Movie, CancellationToken>((indexed, _) => indexedMovie = indexed)
            .Returns(Task.CompletedTask);

        await _handler.Handle(command, CancellationToken.None);

        var reindexed = Assert.IsType<Movie>(indexedMovie);
        Assert.Equal(movie.Id, reindexed.Id);
        Assert.Equal(command.Title, reindexed.Title);
    }

    [Fact]
    public async Task Handle_ReturnsNullAndSkipsTheSideEffectsWhenMovieDoesNotExist()
    {
        var movieId = Guid.NewGuid();
        _movieRepository.Setup(repository => repository.GetByIdAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync((Movie?)null);

        var result = await _handler.Handle(CreateCommand(movieId), CancellationToken.None);

        Assert.Null(result);
        _unitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _movieSearchService.Verify(service => service.IndexAsync(It.IsAny<Movie>(), It.IsAny<CancellationToken>()), Times.Never);
        _movieRepository.Verify(repository => repository.Add(It.IsAny<Movie>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ForwardsCancellationTokenToRepositoryUnitOfWorkAndSearchService()
    {
        var movie = CreateMovie();
        using var cancellationTokenSource = new CancellationTokenSource();
        _movieRepository.Setup(repository => repository.GetByIdAsync(movie.Id, cancellationTokenSource.Token)).ReturnsAsync(movie);
        _unitOfWork.Setup(unitOfWork => unitOfWork.SaveChangesAsync(cancellationTokenSource.Token)).ReturnsAsync(1);

        await _handler.Handle(CreateCommand(movie.Id), cancellationTokenSource.Token);

        _movieRepository.Verify(repository => repository.GetByIdAsync(movie.Id, cancellationTokenSource.Token), Times.Once);
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

    private static UpdateMovieCommand CreateCommand(Guid movieId)
    {
        return new UpdateMovieCommand
        {
            Id = movieId,
            Title = "Interstellar",
            Year = 2014,
            Description = "A team travels through a wormhole in search of a new home for humanity.",
            Director = "Christopher Nolan",
            Overview = "Earth's resources are being depleted.",
            OriginalLanguage = "en",
            PosterUrl = "https://example.com/posters/interstellar.jpg",
        };
    }

    [Fact]
    public async Task Handle_ReplacesGenresWithTheRequestedNames()
    {
        var movie = CreateMovie();
        movie.Genres.Add(new Genre { Name = "Drama" });
        _movieRepository.Setup(repository => repository.GetByIdAsync(movie.Id, It.IsAny<CancellationToken>())).ReturnsAsync(movie);
        _genreRepository
            .Setup(repository => repository.GetByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Genre { Name = "Thriller" }]);

        var result = await _handler.Handle(CreateCommand(movie.Id) with { Genres = ["Thriller", "Mystery"] }, CancellationToken.None);

        var updatedMovie = Assert.IsType<Movie>(result);
        Assert.Equal(["Mystery", "Thriller"], updatedMovie.Genres.Select(genre => genre.Name).OrderBy(name => name));
    }
}
