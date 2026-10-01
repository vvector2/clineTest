using ClineTestCore.Models;
using MediatR;

namespace ClineTestApp.Queries.SearchMovies;

public sealed record SearchMoviesQuery(string Query) : IRequest<IEnumerable<Movie>>;
