import { useCallback, useState } from 'react';
import type { Movie } from '../types/movie';
import { PosterFallbackIcon } from './icons';

interface MovieCardProps {
  movie: Movie;
  onSelect: (movie: Movie) => void;
}

export function MovieCard({ movie, onSelect }: MovieCardProps) {
  const [hasPosterError, setHasPosterError] = useState(false);

  const handleSelect = useCallback(() => {
    onSelect(movie);
  }, [movie, onSelect]);

  const handleImageError = useCallback(() => {
    setHasPosterError(true);
  }, []);

  return (
    <button
      type="button"
      onClick={handleSelect}
      aria-label={`Show details for ${movie.title} (${movie.year})`}
      className="group relative block w-full overflow-hidden rounded-md bg-neutral-800 text-left transition duration-200 hover:scale-105 focus-visible:scale-105 focus-visible:ring-2 focus-visible:ring-red-600 focus-visible:outline-none"
    >
      {hasPosterError ? (
        <span className="flex aspect-2/3 w-full flex-col items-center justify-center gap-2 bg-neutral-800 text-neutral-500">
          <PosterFallbackIcon />
          <span className="px-2 text-center text-xs">No poster</span>
        </span>
      ) : (
        <img
          src={movie.posterUrl}
          alt={`Poster of ${movie.title}`}
          loading="lazy"
          onError={handleImageError}
          className="aspect-2/3 w-full object-cover"
        />
      )}
      <span className="absolute inset-x-0 bottom-0 bg-gradient-to-t from-black via-black/80 to-transparent p-2 opacity-0 transition-opacity group-hover:opacity-100 group-focus-visible:opacity-100">
        <span className="block truncate text-sm font-semibold text-white">{movie.title}</span>
        <span className="block text-xs text-neutral-400">{movie.year}</span>
      </span>
    </button>
  );
}
