import { useMemo } from 'react';
import type { Movie } from '../types/movie';
import { groupMoviesByYear } from '../utils/groupMoviesByYear';
import { EmptyState } from './EmptyState';
import { ErrorMessage } from './ErrorMessage';
import { MovieCard } from './MovieCard';
import { MovieRow } from './MovieRow';
import { MoviesSkeleton } from './MoviesSkeleton';

interface MoviesCatalogProps {
  movies: Movie[];
  isLoading: boolean;
  error: string | null;
  searchPhrase: string;
  onRetry: () => void;
  onSelect: (movie: Movie) => void;
}

export function MoviesCatalog({
  movies,
  isLoading,
  error,
  searchPhrase,
  onRetry,
  onSelect,
}: MoviesCatalogProps) {
  const rows = useMemo(() => groupMoviesByYear(movies), [movies]);
  const phrase = searchPhrase.trim();
  const isSearching = phrase.length > 0;

  if (isLoading) {
    return <MoviesSkeleton />;
  }

  if (error !== null) {
    return <ErrorMessage message={error} onRetry={onRetry} />;
  }

  if (movies.length === 0) {
    return <EmptyState searchPhrase={searchPhrase} />;
  }

  if (isSearching) {
    return (
      <section aria-labelledby="search-results-heading" className="px-4">
        <h2 id="search-results-heading" className="mb-4 text-lg font-semibold text-neutral-100">
          {movies.length} {movies.length === 1 ? 'result' : 'results'} for{' '}
          <span className="text-red-500">{phrase}</span>
        </h2>
        <ul className="grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6">
          {movies.map((movie) => (
            <li key={movie.id}>
              <MovieCard movie={movie} onSelect={onSelect} />
            </li>
          ))}
        </ul>
      </section>
    );
  }

  return (
    <div className="space-y-10">
      {rows.map((row) => (
        <MovieRow key={row.year} title={String(row.year)} movies={row.movies} onSelect={onSelect} />
      ))}
    </div>
  );
}
