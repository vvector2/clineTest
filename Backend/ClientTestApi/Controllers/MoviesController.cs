using ClientTestApi.Models;
using ClineTestApp.Commands.CreateMovie;
using ClineTestApp.Commands.UpdateMoviePoster;
using ClineTestApp.Queries.GetMovieById;
using ClineTestApp.Queries.GetMovies;
using ClineTestApp.Queries.SearchMovies;
using ClineTestCore.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClientTestApi.Controllers;

[ApiController]
[Route("[controller]")]
public class MoviesController : ControllerBase
{
    private const long MaxPosterSizeInBytes = 10 * 1024 * 1024;

    private readonly IMediator _mediator;

    public MoviesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet(Name = "GetMovies")]
    public async Task<ActionResult<IEnumerable<Movie>>> GetAsync([FromQuery] string? query, CancellationToken cancellationToken)
    {
        var movies = string.IsNullOrWhiteSpace(query)
            ? await _mediator.Send(new GetMoviesQuery(), cancellationToken)
            : await _mediator.Send(new SearchMoviesQuery(query), cancellationToken);

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

    [HttpPut("poster", Name = "UpdateMoviePoster")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UpdateMoviePosterResponse>> UpdatePosterAsync(
        [FromForm] UpdateMoviePosterRequest request,
        CancellationToken cancellationToken
    )
    {
        if (request.Poster.Length == 0)
        {
            ModelState.AddModelError(nameof(request.Poster), "The poster file must not be empty.");
        }

        if (request.Poster.Length > MaxPosterSizeInBytes)
        {
            ModelState.AddModelError(nameof(request.Poster), $"The poster file must not exceed {MaxPosterSizeInBytes / (1024 * 1024)} megabytes.");
        }

        if (!request.Poster.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(request.Poster), "The poster file must be an image.");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        await using var content = new MemoryStream();

        await request.Poster.CopyToAsync(content, cancellationToken);
        content.Position = 0;

        var command = new UpdateMoviePosterCommand
        {
            MovieId = request.MovieId!.Value,
            Content = content,
            FileName = request.Poster.FileName,
            ContentType = request.Poster.ContentType,
        };

        var posterUrl = await _mediator.Send(command, cancellationToken);

        if (posterUrl is null)
        {
            return NotFound();
        }

        return Ok(new UpdateMoviePosterResponse(posterUrl));
    }
}
