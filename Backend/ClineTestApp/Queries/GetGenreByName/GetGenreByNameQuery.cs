using ClineTestCore.Models;
using MediatR;

namespace ClineTestApp.Queries.GetGenreByName;

public sealed record GetGenreByNameQuery(string Name) : IRequest<Genre?>;
