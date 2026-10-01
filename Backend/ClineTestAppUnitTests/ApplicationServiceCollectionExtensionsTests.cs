using ClineTestApp;
using ClineTestApp.Commands.CreateMovie;
using ClineTestApp.Commands.UpdateMoviePoster;
using ClineTestApp.Queries.GetMovieById;
using ClineTestApp.Queries.GetMovies;
using ClineTestApp.Queries.SearchMovies;
using ClineTestCore.Models;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ClineTestAppUnitTests;

public class ApplicationServiceCollectionExtensionsTests
{
    [Fact]
    public void AddApplication_ThrowsArgumentNullExceptionWhenServicesIsNull()
    {
        IServiceCollection? services = null;

        Assert.Throws<ArgumentNullException>(() => services!.AddApplication());
    }

    [Fact]
    public void AddApplication_ReturnsTheSameServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddApplication();

        Assert.Same(services, result);
    }

    [Fact]
    public void AddApplication_RegistersAllApplicationHandlers()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IRequestHandler<GetMoviesQuery, IEnumerable<Movie>>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IRequestHandler<SearchMoviesQuery, IEnumerable<Movie>>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IRequestHandler<GetMovieByIdQuery, Movie?>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IRequestHandler<CreateMovieCommand, Guid>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IRequestHandler<UpdateMoviePosterCommand, string?>));
    }

    [Fact]
    public void AddApplication_ResolvesMediator()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        using var provider = services.BuildServiceProvider();

        var mediator = provider.GetService<IMediator>();

        Assert.NotNull(mediator);
    }
}
