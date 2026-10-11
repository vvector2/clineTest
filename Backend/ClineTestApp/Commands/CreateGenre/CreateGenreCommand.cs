using System.ComponentModel.DataAnnotations;
using MediatR;

namespace ClineTestApp.Commands.CreateGenre;

public sealed record CreateGenreCommand : IRequest<string>
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; init; }
}
