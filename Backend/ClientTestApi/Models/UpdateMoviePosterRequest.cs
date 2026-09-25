using System.ComponentModel.DataAnnotations;

namespace ClientTestApi.Models;

public sealed class UpdateMoviePosterRequest
{
    [Required]
    public Guid MovieId { get; init; }

    [Required]
    public required IFormFile Poster { get; init; }
}
