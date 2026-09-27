using ClineTestApp.Queries.GetMovies;
using ClineTestCore.Models;
using ClineTestCore.Repositories;
using Moq;

namespace ClineTestAppUnitTests;

public class GetMoviesQueryHandlerTests
{
    private readonly Mock<IMovieRepository> _movieRepository = new();
    private readonly GetMoviesQueryHandler _handler;

    public GetMoviesQueryHandlerTests()
    {
        _handler = new GetMoviesQueryHandler(_movieRepository.Object);
    }

    [Fact]
    public async Task Handle_ReturnsMoviesFromRepository()
    {
        var movies = new List<Movie>
        {
            new Movie
            {
                Id = Guid.NewGuid(),
                Title = "Inception",
                Year = 2010,
                Description = "A thief who steals corporate secrets.",
                Director = "Christopher Nolan",
                Overview = "Dreams within dreams.",
                OriginalLanguage = "en",
                PosterUrl = "https://example.com/posters/inception.jpg",
            },
        };
        _movieRepository.Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(movies);

        var result = await _handler.Handle(new GetMoviesQuery(), CancellationToken.None);

        Assert.Same(movies, result);
    }

    [Fact]
    public async Task Handle_ForwardsCancellationTokenToRepository()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        _movieRepository.Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);

        await _handler.Handle(new GetMoviesQuery(), cancellationTokenSource.Token);

        _movieRepository.Verify(repository => repository.GetAllAsync(cancellationTokenSource.Token), Times.Once);
    }
}
