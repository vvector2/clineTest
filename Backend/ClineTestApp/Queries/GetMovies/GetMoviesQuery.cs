using ClineTestCore.Models;
using MediatR;

namespace ClineTestApp.Queries.GetMovies;

public sealed record GetMoviesQuery : IRequest<IEnumerable<Movie>>;
