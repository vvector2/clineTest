using ClineTestCore.Models;

namespace ClineTestCoreUnitTests;

public class MovieTests
{
    [Fact]
    public void Properties_RoundTripAssignedValues()
    {
        var id = Guid.NewGuid();
        var movie = new Movie
        {
            Id = id,
            Title = "Inception",
            Year = 2010,
            Description = "A thief who steals corporate secrets through dream-sharing technology.",
            Director = "Christopher Nolan",
            Overview = "Dom Cobb is a skilled thief in the art of extraction.",
            OriginalLanguage = "en",
            PosterUrl = "https://example.com/posters/inception.jpg",
        };

        Assert.Equal(id, movie.Id);
        Assert.Equal("Inception", movie.Title);
        Assert.Equal(2010, movie.Year);
        Assert.Equal("A thief who steals corporate secrets through dream-sharing technology.", movie.Description);
        Assert.Equal("Christopher Nolan", movie.Director);
        Assert.Equal("Dom Cobb is a skilled thief in the art of extraction.", movie.Overview);
        Assert.Equal("en", movie.OriginalLanguage);
        Assert.Equal("https://example.com/posters/inception.jpg", movie.PosterUrl);
    }

    [Fact]
    public void OptionalProperties_UseDefaultValuesWhenNotAssigned()
    {
        var movie = CreateMovie();

        Assert.Equal(Guid.Empty, movie.Id);
        Assert.Equal(0, movie.Year);
        Assert.Equal("Interstellar", movie.Title);
    }

    [Fact]
    public void MutatingOneMovie_DoesNotAffectAnother()
    {
        var movie = CreateMovie();
        var other = CreateMovie();
        var newId = Guid.NewGuid();

        movie.Id = newId;
        movie.Year = 2014;
        movie.PosterUrl = "https://example.com/posters/interstellar-updated.jpg";

        Assert.Equal(newId, movie.Id);
        Assert.Equal(2014, movie.Year);
        Assert.Equal("https://example.com/posters/interstellar-updated.jpg", movie.PosterUrl);
        Assert.Equal(Guid.Empty, other.Id);
        Assert.Equal(0, other.Year);
        Assert.Equal("https://example.com/posters/interstellar.jpg", other.PosterUrl);
    }

    private static Movie CreateMovie()
    {
        return new Movie
        {
            Title = "Interstellar",
            Description = "A team travels through a wormhole in search of a new home for humanity.",
            Director = "Christopher Nolan",
            Overview = "Earth's resources are being depleted.",
            OriginalLanguage = "en",
            PosterUrl = "https://example.com/posters/interstellar.jpg",
        };
    }
}
