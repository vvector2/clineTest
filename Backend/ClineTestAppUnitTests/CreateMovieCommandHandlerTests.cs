using ClineTestApp.Commands.CreateMovie;
using ClineTestCore.Models;
using ClineTestCore.Repositories;
using ClineTestCore.Search;
using Moq;

namespace ClineTestAppUnitTests;

public class CreateMovieCommandHandlerTests
{
    private readonly Mock<IMovieRepository> _movieRepository = new();
    private readonly Mock<IMovieSearchService> _movieSearchService = new();
    private readonly CreateMovieCommandHandler _handler;

    public CreateMovieCommandHandlerTests()
    {
        _handler = new CreateMovieCommandHandler(_movieRepository.Object, _movieSearchService.Object);
    }

    [Fact]
    public async Task Handle_CreatesMovieFromCommandAndReturnsGeneratedId()
    {
        Movie? persistedMovie = null;
        _movieRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Movie>(), It.IsAny<CancellationToken>()))
            .Callback<Movie, CancellationToken>((movie, _) => persistedMovie = movie)
            .Returns(Task.CompletedTask);
        var command = new CreateMovieCommand
        {
            Title = "Inception",
            Year = 2010,
            Description = "A thief who steals corporate secrets through dream-sharing technology.",
            Director = "Christopher Nolan",
            Overview = "Dom Cobb is a skilled thief in the art of extraction.",
            OriginalLanguage = "en",
            PosterUrl = "https://example.com/posters/inception.jpg",
        };

        var movieId = await _handler.Handle(command, CancellationToken.None);

        var movie = Assert.IsType<Movie>(persistedMovie);
        Assert.NotEqual(Guid.Empty, movieId);
        Assert.Equal(movieId, movie.Id);
        Assert.Equal(command.Title, movie.Title);
        Assert.Equal(command.Year, movie.Year);
        Assert.Equal(command.Description, movie.Description);
        Assert.Equal(command.Director, movie.Director);
        Assert.Equal(command.Overview, movie.Overview);
        Assert.Equal(command.OriginalLanguage, movie.OriginalLanguage);
        Assert.Equal(command.PosterUrl, movie.PosterUrl);
    }

    [Fact]
    public async Task Handle_IndexesTheCreatedMovieInTheSearchService()
    {
        Movie? indexedMovie = null;
        _movieSearchService
            .Setup(service => service.IndexAsync(It.IsAny<Movie>(), It.IsAny<CancellationToken>()))
            .Callback<Movie, CancellationToken>((movie, _) => indexedMovie = movie)
            .Returns(Task.CompletedTask);

        var movieId = await _handler.Handle(CreateCommand(), CancellationToken.None);

        var movie = Assert.IsType<Movie>(indexedMovie);
        Assert.Equal(movieId, movie.Id);
    }

    [Fact]
    public async Task Handle_ForwardsCancellationTokenToRepositoryAndSearchService()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        _movieSearchService.Setup(service => service.IndexAsync(It.IsAny<Movie>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _handler.Handle(CreateCommand(), cancellationTokenSource.Token);

        _movieRepository.Verify(repository => repository.AddAsync(It.IsAny<Movie>(), cancellationTokenSource.Token), Times.Once);
        _movieSearchService.Verify(service => service.IndexAsync(It.IsAny<Movie>(), cancellationTokenSource.Token), Times.Once);
    }

    private static CreateMovieCommand CreateCommand()
    {
        return new CreateMovieCommand
        {
            Title = "Interstellar",
            Year = 2014,
            Description = "A team travels through a wormhole in search of a new home for humanity.",
            Director = "Christopher Nolan",
            Overview = "Earth's resources are being depleted.",
            OriginalLanguage = "en",
            PosterUrl = "https://example.com/posters/interstellar.jpg",
        };
    }
}
