import type { Movie } from '../types/movie';

const DEFAULT_API_BASE_URL = '/api';
const TRAILING_SLASHES = /\/+$/;

const configuredBaseUrl = import.meta.env.VITE_API_BASE_URL?.trim() ?? '';
const API_BASE_URL = configuredBaseUrl.length > 0 ? configuredBaseUrl.replace(TRAILING_SLASHES, '') : DEFAULT_API_BASE_URL;

export class MoviesApiError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'MoviesApiError';
  }
}

function buildMoviesUrl(query: string): string {
  const phrase = query.trim();
  const url = `${API_BASE_URL}/movies`;

  return phrase.length === 0 ? url : `${url}?query=${encodeURIComponent(phrase)}`;
}

function isMovie(value: unknown): value is Movie {
  if (typeof value !== 'object' || value === null) {
    return false;
  }

  const candidate = value as Record<string, unknown>;

  return (
    typeof candidate.id === 'string' &&
    typeof candidate.title === 'string' &&
    typeof candidate.year === 'number' &&
    typeof candidate.posterUrl === 'string'
  );
}

function isAbortError(error: unknown): boolean {
  return error instanceof DOMException && error.name === 'AbortError';
}

export async function getMovies(query: string, signal?: AbortSignal): Promise<Movie[]> {
  let response: Response;

  try {
    response = await fetch(buildMoviesUrl(query), {
      signal,
      headers: { Accept: 'application/json' },
    });
  } catch (error: unknown) {
    if (isAbortError(error)) {
      throw error;
    }

    throw new MoviesApiError('Could not reach the movie service. Check your connection and try again.');
  }

  if (!response.ok) {
    throw new MoviesApiError(`The movie service responded with status ${response.status}.`);
  }

  const payload: unknown = await response.json();
  const movies = Array.isArray(payload) ? payload.filter(isMovie) : [];

  if (!Array.isArray(payload) || movies.length !== payload.length) {
    throw new MoviesApiError('The movie service returned an unexpected response.');
  }

  return movies;
}
