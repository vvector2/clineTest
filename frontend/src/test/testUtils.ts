import type { Movie } from '../types/movie';

export const fetchMock = vi.fn();

export function stubFetch(): void {
  vi.stubGlobal('fetch', fetchMock);
}

export function createJsonResponse(body: unknown, ok = true, status = 200): Response {
  return {
    ok,
    status,
    json: () => Promise.resolve(body),
  } as Response;
}

export function createMovie(overrides: Partial<Movie> = {}): Movie {
  return {
    id: '11111111-1111-1111-1111-111111111111',
    title: 'The Matrix',
    year: 1999,
    description: 'A hacker discovers that reality is a simulation.',
    director: 'Lana Wachowski',
    overview: 'Neo learns what the Matrix really is.',
    originalLanguage: 'English',
    posterUrl: 'http://localhost:4566/clinetest-posters/movies/1111.jpg',
    genres: [],
    ...overrides,
  };
}

export function createMovies(count: number, overrides: Partial<Movie> = {}): Movie[] {
  return Array.from({ length: count }, (_, index) =>
    createMovie({ id: `movie-${index + 1}`, title: `Movie ${index + 1}`, ...overrides }),
  );
}
