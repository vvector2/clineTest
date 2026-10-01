import { useCallback, useEffect, useState } from 'react';
import { getMovies } from '../api/moviesApi';
import type { Movie } from '../types/movie';
import { useDebouncedValue } from './useDebouncedValue';

const SEARCH_DEBOUNCE_MS = 300;
const FALLBACK_ERROR_MESSAGE = 'Something went wrong while loading movies.';

interface MoviesState {
  requestKey: string;
  movies: Movie[];
  error: string | null;
  isSettled: boolean;
}

export interface UseMoviesResult {
  movies: Movie[];
  isLoading: boolean;
  error: string | null;
  reload: () => void;
}

const initialState: MoviesState = {
  requestKey: '',
  movies: [],
  error: null,
  isSettled: false,
};

export function useMovies(searchPhrase: string): UseMoviesResult {
  const debouncedSearchPhrase = useDebouncedValue(searchPhrase, SEARCH_DEBOUNCE_MS);
  const [reloadToken, setReloadToken] = useState(0);
  const requestKey = `${debouncedSearchPhrase}:${reloadToken}`;
  const [state, setState] = useState<MoviesState>(initialState);

  const reload = useCallback(() => {
    setReloadToken((currentToken) => currentToken + 1);
  }, []);

  useEffect(() => {
    const controller = new AbortController();

    const loadMovies = async (): Promise<void> => {
      try {
        const movies = await getMovies(debouncedSearchPhrase, controller.signal);

        if (controller.signal.aborted) {
          return;
        }

        setState({ requestKey, movies, error: null, isSettled: true });
      } catch (caught: unknown) {
        if (controller.signal.aborted) {
          return;
        }

        setState({
          requestKey,
          movies: [],
          error: caught instanceof Error ? caught.message : FALLBACK_ERROR_MESSAGE,
          isSettled: true,
        });
      }
    };

    void loadMovies();

    return () => {
      controller.abort();
    };
  }, [debouncedSearchPhrase, requestKey]);

  const isCurrentRequest = state.requestKey === requestKey;

  return {
    movies: isCurrentRequest ? state.movies : [],
    isLoading: !isCurrentRequest || !state.isSettled,
    error: isCurrentRequest ? state.error : null,
    reload,
  };
}
