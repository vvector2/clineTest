using ClineTestCore.Models;
using Microsoft.EntityFrameworkCore;

namespace ClineTestInfra.DAL;

public class ClineTestDbContext : DbContext
{
    public static readonly Guid ShawshankMovieId = Guid.Parse("3f2504e0-4f89-41d3-9a0c-0305e82c3301");
    public static readonly Guid GodfatherMovieId = Guid.Parse("3f2504e0-4f89-41d3-9a0c-0305e82c3302");
    public static readonly Guid ParasiteMovieId = Guid.Parse("3f2504e0-4f89-41d3-9a0c-0305e82c3303");

    public ClineTestDbContext(DbContextOptions<ClineTestDbContext> options)
        : base(options) { }

    public DbSet<Movie> Movies => Set<Movie>();

    public DbSet<Genre> Genres => Set<Genre>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var movie = modelBuilder.Entity<Movie>();

        movie.ToTable("movies");
        movie.HasKey(m => m.Id);
        movie.Property(m => m.Title).HasMaxLength(200);
        movie.Property(m => m.Description).HasMaxLength(1000);
        movie.Property(m => m.Director).HasMaxLength(200);
        movie.Property(m => m.Overview).HasMaxLength(2000);
        movie.Property(m => m.OriginalLanguage).HasMaxLength(100);
        movie.Property(m => m.PosterUrl).HasMaxLength(500);

        var genre = modelBuilder.Entity<Genre>();

        genre.ToTable("genres");
        genre.HasKey(g => g.Name);
        genre.Property(g => g.Name).HasMaxLength(100).IsRequired();

        movie
            .HasMany(m => m.Genres)
            .WithMany(g => g.Movies)
            .UsingEntity<Dictionary<string, object>>(
                "MovieGenre",
                right =>
                    right
                        .HasOne<Genre>()
                        .WithMany()
                        .HasForeignKey("GenreName")
                        .HasConstraintName("FK_movie_genres_genres")
                        .OnDelete(DeleteBehavior.Cascade),
                left =>
                    left.HasOne<Movie>()
                        .WithMany()
                        .HasForeignKey("MoviesId")
                        .HasConstraintName("FK_movie_genres_movies")
                        .OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.ToTable("movie_genres");
                    join.HasKey("MoviesId", "GenreName");
                    join.HasData(
                        new { MoviesId = ShawshankMovieId, GenreName = "Drama" },
                        new { MoviesId = ShawshankMovieId, GenreName = "Crime" },
                        new { MoviesId = GodfatherMovieId, GenreName = "Crime" },
                        new { MoviesId = GodfatherMovieId, GenreName = "Drama" },
                        new { MoviesId = ParasiteMovieId, GenreName = "Thriller" },
                        new { MoviesId = ParasiteMovieId, GenreName = "Drama" }
                    );
                }
            );

        movie.HasData(
            new Movie
            {
                Id = ShawshankMovieId,
                Title = "The Shawshank Redemption",
                Year = 1994,
                Description = "Two imprisoned men bond over a number of years.",
                Director = "Frank Darabont",
                Overview = "A banker convicted of uxoricide forms a friendship over a quarter century with a hardened convict.",
                OriginalLanguage = "English",
                PosterUrl = "https://image.tmdb.org/t/p/w500/q6y0Go1tsGEsmtFryDOJo3dEmqu.jpg",
            },
            new Movie
            {
                Id = GodfatherMovieId,
                Title = "The Godfather",
                Year = 1972,
                Description = "The aging patriarch of an organized crime dynasty transfers control to his reluctant son.",
                Director = "Francis Ford Coppola",
                Overview = "The story of the Corleone family under patriarch Vito Corleone.",
                OriginalLanguage = "English",
                PosterUrl = "https://image.tmdb.org/t/p/w500/3bhkrj58Vtu7enYsRolD1fZdja1.jpg",
            },
            new Movie
            {
                Id = ParasiteMovieId,
                Title = "Parasite",
                Year = 2019,
                Description = "Greed and class discrimination threaten the newly formed symbiotic relationship between two families.",
                Director = "Bong Joon-ho",
                Overview = "A poor family schemes to become employed by a wealthy family by infiltrating their household.",
                OriginalLanguage = "Korean",
                PosterUrl = "https://image.tmdb.org/t/p/w500/7IiTTgloJzvGI1TAYymCfbfl3vT.jpg",
            }
        );

        genre.HasData(new Genre { Name = "Drama" }, new Genre { Name = "Crime" }, new Genre { Name = "Thriller" });
    }
}
