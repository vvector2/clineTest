using System.ComponentModel.DataAnnotations;
using ClineTestCore.Models;
using MediatR;

namespace ClineTestApp.Commands.UpdateMovie;

public sealed record UpdateMovieCommand : IRequest<Movie?>
{
    public Guid Id { get; init; }

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

    [Required]
    [Url]
    [MaxLength(500)]
    public required string PosterUrl { get; init; }

    public List<string> Genres { get; init; } = new();
}
