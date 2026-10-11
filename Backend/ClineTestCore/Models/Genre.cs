namespace ClineTestCore.Models;

public class Genre
{
    public required string Name { get; set; }
    public ICollection<Movie> Movies { get; set; } = new List<Movie>();
}
