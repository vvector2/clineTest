import { act, renderHook, waitFor } from '@testing-library/react';
import { useMovies } from '../hooks/useMovies';
import { createJsonResponse, createMovie, fetchMock, stubFetch } from './testUtils';

describe('useMovies', () => {
  beforeEach(() => {
    stubFetch();
  });

  afterEach(() => {
    fetchMock.mockReset();
  });

  it('loads the catalogue and exposes the movies', async () => {
    const movie = createMovie();
    fetchMock.mockResolvedValue(createJsonResponse([movie]));

    const { result } = renderHook(() => useMovies(''));

    expect(result.current.isLoading).toBe(true);

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.movies).toEqual([movie]);
    expect(result.current.error).toBeNull();
  });

  it('requests a filtered catalogue once the phrase is debounced', async () => {
    fetchMock.mockResolvedValue(createJsonResponse([]));

    const { result } = renderHook(() => useMovies('matrix'));
    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(fetchMock).toHaveBeenCalledWith('/api/movies?query=matrix', expect.any(Object));
  });

  it('surfaces the error message when the request fails', async () => {
    fetchMock.mockResolvedValue(createJsonResponse([], false, 503));

    const { result } = renderHook(() => useMovies(''));
    await waitFor(() => expect(result.current.error).not.toBeNull());

    expect(result.current.error).toBe('The movie service responded with status 503.');
    expect(result.current.movies).toEqual([]);
    expect(result.current.isLoading).toBe(false);
  });

  it('surfaces a network failure message', async () => {
    fetchMock.mockRejectedValue(new TypeError('Failed to fetch'));

    const { result } = renderHook(() => useMovies(''));
    await waitFor(() => expect(result.current.error).not.toBeNull());

    expect(result.current.error).toBe(
      'Could not reach the movie service. Check your connection and try again.',
    );
  });

  it('reloads the catalogue on demand', async () => {
    fetchMock.mockResolvedValue(createJsonResponse([]));

    const { result } = renderHook(() => useMovies(''));
    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(fetchMock).toHaveBeenCalledTimes(1);

    act(() => {
      result.current.reload();
    });

    await waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(2));
  });

  it('ignores a response that arrives after the request was aborted', async () => {
    const movie = createMovie();
    fetchMock.mockResolvedValue(createJsonResponse([movie]));

    const { result, unmount } = renderHook(() => useMovies('matrix'));
    unmount();

    await waitFor(() => expect(result.current.isLoading).toBe(true));
  });
});
