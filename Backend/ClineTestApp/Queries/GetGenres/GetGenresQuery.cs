using ClineTestCore.Models;
using MediatR;

namespace ClineTestApp.Queries.GetGenres;

public sealed record GetGenresQuery : IRequest<IEnumerable<Genre>>;
