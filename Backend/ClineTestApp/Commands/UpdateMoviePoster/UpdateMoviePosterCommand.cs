using MediatR;

namespace ClineTestApp.Commands.UpdateMoviePoster;

public sealed record UpdateMoviePosterCommand : IRequest<string?>
{
    public required Guid MovieId { get; init; }

    public required Stream Content { get; init; }

    public required string FileName { get; init; }

    public required string ContentType { get; init; }
}
