using ClineTestApp.Commands.CreateGenre;
using ClineTestApp.Queries.GetGenreByName;
using ClineTestApp.Queries.GetGenres;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClientTestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenresController : ControllerBase
{
    private readonly IMediator _mediator;

    public GenresController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet(Name = "GetGenres")]
    public async Task<ActionResult<IEnumerable<string>>> GetAsync(CancellationToken cancellationToken)
    {
        var genres = await _mediator.Send(new GetGenresQuery(), cancellationToken);

        return Ok(genres.Select(genre => genre.Name));
    }

    [HttpGet("{name}", Name = "GetGenreByName")]
    public async Task<ActionResult<string>> GetByNameAsync(string name, CancellationToken cancellationToken)
    {
        var genre = await _mediator.Send(new GetGenreByNameQuery(name), cancellationToken);

        if (genre is null)
        {
            return NotFound();
        }

        return Ok(genre.Name);
    }

    [HttpPost(Name = "CreateGenre")]
    public async Task<ActionResult<string>> CreateAsync(CreateGenreCommand command, CancellationToken cancellationToken)
    {
        var genreName = await _mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, genreName);
    }
}
