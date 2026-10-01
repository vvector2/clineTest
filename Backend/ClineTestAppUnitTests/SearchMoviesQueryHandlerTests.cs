using ClineTestApp.Queries.SearchMovies;
using ClineTestCore.Models;
using ClineTestCore.Search;
using Moq;

namespace ClineTestAppUnitTests;

public class SearchMoviesQueryHandlerTests
{
    private readonly Mock<IMovieSearchService> _movieSearchService = new();
    private readonly SearchMoviesQueryHandler _handler;

    public SearchMoviesQueryHandlerTests()
    {
        _handler = new SearchMoviesQueryHandler(_movieSearchService.Object);
    }

    [Fact]
    public async Task Handle_ReturnsMoviesFromSearchService()
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
        _movieSearchService.Setup(service => service.SearchAsync("inception", It.IsAny<CancellationToken>())).ReturnsAsync(movies);

        var result = await _handler.Handle(new SearchMoviesQuery("inception"), CancellationToken.None);

        Assert.Same(movies, result);
    }

    [Fact]
    public async Task Handle_ForwardsQueryAndCancellationTokenToSearchService()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        _movieSearchService.Setup(service => service.SearchAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Movie>());

        await _handler.Handle(new SearchMoviesQuery("matrix"), cancellationTokenSource.Token);

        _movieSearchService.Verify(service => service.SearchAsync("matrix", cancellationTokenSource.Token), Times.Once);
    }
}
