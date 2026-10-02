import { afterEach, describe, expect, it, vi } from 'vitest';
import { getMovies, MoviesApiError } from '../api/moviesApi';
import { createJsonResponse, createMovie, fetchMock, stubFetch } from './testUtils';

async function fetchUrlForBaseUrl(baseUrl: string | undefined): Promise<string> {
  vi.stubEnv('VITE_API_BASE_URL', baseUrl ?? '');
  vi.resetModules();
  stubFetch();
  fetchMock.mockResolvedValue(createJsonResponse([]));

  const module = await import('../api/moviesApi');
  await module.getMovies('');

  return fetchMock.mock.calls[0][0] as string;
}

describe('getMovies', () => {
  afterEach(() => {
    fetchMock.mockReset();
  });

  it('requests the plain movies endpoint for a blank phrase', async () => {
    stubFetch();
    fetchMock.mockResolvedValue(createJsonResponse([]));

    await getMovies('   ');

    expect(fetchMock).toHaveBeenCalledWith('/api/movies', expect.any(Object));
  });

  it('appends an encoded query parameter for a search phrase', async () => {
    stubFetch();
    fetchMock.mockResolvedValue(createJsonResponse([]));

    await getMovies('the matrix');

    expect(fetchMock).toHaveBeenCalledWith('/api/movies?query=the%20matrix', expect.any(Object));
  });

  it('sends an accept header and the abort signal', async () => {
    stubFetch();
    fetchMock.mockResolvedValue(createJsonResponse([]));
    const controller = new AbortController();

    await getMovies('alien', controller.signal);

    expect(fetchMock).toHaveBeenCalledWith(
      '/api/movies?query=alien',
      expect.objectContaining({
        signal: controller.signal,
        headers: { Accept: 'application/json' },
      }),
    );
  });

  it('returns the movies from a successful response', async () => {
    stubFetch();
    const movie = createMovie();
    fetchMock.mockResolvedValue(createJsonResponse([movie]));

    await expect(getMovies('')).resolves.toEqual([movie]);
  });

  it('throws a movies api error for an unsuccessful status code', async () => {
    stubFetch();
    fetchMock.mockResolvedValue(createJsonResponse([], false, 500));

    await expect(getMovies('')).rejects.toThrow(MoviesApiError);
    await expect(getMovies('')).rejects.toThrow('status 500');
  });

  it('throws a movies api error when the payload is not an array', async () => {
    stubFetch();
    fetchMock.mockResolvedValue(createJsonResponse({ message: 'nope' }));

    await expect(getMovies('')).rejects.toThrow('unexpected response');
  });

  it('throws a movies api error when an item has an unexpected shape', async () => {
    stubFetch();
    fetchMock.mockResolvedValue(createJsonResponse([{ id: 'a', title: 'A' }]));

    await expect(getMovies('')).rejects.toThrow('unexpected response');
  });

  it('throws a movies api error when the network call fails', async () => {
    stubFetch();
    fetchMock.mockRejectedValue(new TypeError('Failed to fetch'));

    await expect(getMovies('')).rejects.toThrow('Could not reach the movie service');
  });

  it('rethrows an abort error so callers can ignore it', async () => {
    stubFetch();
    const abortError = new DOMException('The operation was aborted.', 'AbortError');
    fetchMock.mockRejectedValue(abortError);

    await expect(getMovies('', new AbortController().signal)).rejects.toBe(abortError);
  });
});

describe('getMovies base url', () => {
  afterEach(() => {
    fetchMock.mockReset();
    vi.unstubAllEnvs();
    vi.resetModules();
  });

  it('falls back to the api prefix when the variable is not set', async () => {
    await expect(fetchUrlForBaseUrl(undefined)).resolves.toBe('/api/movies');
  });

  it('falls back to the api prefix when the variable is blank', async () => {
    await expect(fetchUrlForBaseUrl('')).resolves.toBe('/api/movies');
  });

  it('falls back to the api prefix when the variable is only whitespace', async () => {
    await expect(fetchUrlForBaseUrl('   ')).resolves.toBe('/api/movies');
  });

  it('uses a configured base url', async () => {
    await expect(fetchUrlForBaseUrl('https://localhost:7072')).resolves.toBe('https://localhost:7072/movies');
  });

  it('strips a trailing slash from the configured base url', async () => {
    await expect(fetchUrlForBaseUrl('https://localhost:7072/')).resolves.toBe('https://localhost:7072/movies');
  });
});
