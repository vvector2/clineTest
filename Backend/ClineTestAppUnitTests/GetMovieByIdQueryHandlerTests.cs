using ClineTestApp.Queries.GetMovieById;
using ClineTestCore.Models;
using ClineTestCore.Repositories;
using Moq;

namespace ClineTestAppUnitTests;

public class GetMovieByIdQueryHandlerTests
{
    private readonly Mock<IMovieRepository> _movieRepository = new();
    private readonly GetMovieByIdQueryHandler _handler;

    public GetMovieByIdQueryHandlerTests()
    {
        _handler = new GetMovieByIdQueryHandler(_movieRepository.Object);
    }

    [Fact]
    public async Task Handle_ReturnsMovieFromRepository()
    {
        var movie = new Movie
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
        _movieRepository.Setup(repository => repository.GetByIdAsync(movie.Id, It.IsAny<CancellationToken>())).ReturnsAsync(movie);

        var result = await _handler.Handle(new GetMovieByIdQuery(movie.Id), CancellationToken.None);

        Assert.Same(movie, result);
    }

    [Fact]
    public async Task Handle_ReturnsNullWhenMovieDoesNotExist()
    {
        var movieId = Guid.NewGuid();
        _movieRepository.Setup(repository => repository.GetByIdAsync(movieId, It.IsAny<CancellationToken>())).ReturnsAsync((Movie?)null);

        var result = await _handler.Handle(new GetMovieByIdQuery(movieId), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ForwardsQueryIdAndCancellationTokenToRepository()
    {
        var movieId = Guid.NewGuid();
        using var cancellationTokenSource = new CancellationTokenSource();
        _movieRepository.Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Movie?)null);

        await _handler.Handle(new GetMovieByIdQuery(movieId), cancellationTokenSource.Token);

        _movieRepository.Verify(repository => repository.GetByIdAsync(movieId, cancellationTokenSource.Token), Times.Once);
    }
}
