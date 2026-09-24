using System.ComponentModel.DataAnnotations;
using MediatR;

namespace ClineTestApp.Commands.CreateMovie;

public sealed record CreateMovieCommand : IRequest<Guid>
{
    [Required]
    [MaxLength(200)]
    public required string Title { get; init; }

    public int Year { get; init; }

    [Required]
    [MaxLength(1000)]
    public required string Description { get; init; }

    [Required]
    [MaxLength(200)]
    public required string Director { get; init; }

    [Required]
    [MaxLength(2000)]
    public required string Overview { get; init; }

    [Required]
    [MaxLength(100)]
    public required string OriginalLanguage { get; init; }
}
