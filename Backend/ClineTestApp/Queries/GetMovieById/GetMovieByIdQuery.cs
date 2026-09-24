using ClineTestCore.Models;
using MediatR;

namespace ClineTestApp.Queries.GetMovieById;

public sealed record GetMovieByIdQuery(Guid Id) : IRequest<Movie?>;
