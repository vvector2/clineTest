using ClineTestApp.Commands.CreateMovie;
using ClineTestApp.Queries.GetMovieById;
using ClineTestApp.Queries.GetMovies;
using ClineTestCore.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClientTestApi.Controllers;

[ApiController]
[Route("[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MoviesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet(Name = "GetMovies")]
    public async Task<ActionResult<IEnumerable<Movie>>> GetAsync(CancellationToken cancellationToken)
    {
        var movies = await _mediator.Send(new GetMoviesQuery(), cancellationToken);
        return Ok(movies);
    }

    [HttpGet("{id:guid}", Name = "GetMovieById")]
    public async Task<ActionResult<Movie>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var movie = await _mediator.Send(new GetMovieByIdQuery(id), cancellationToken);

        if (movie is null)
        {
            return NotFound();
        }

        return Ok(movie);
    }

    [HttpPost(Name = "CreateMovie")]
    public async Task<ActionResult<Guid>> CreateAsync(CreateMovieCommand command, CancellationToken cancellationToken)
    {
        var movieId = await _mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, movieId);
    }
}
