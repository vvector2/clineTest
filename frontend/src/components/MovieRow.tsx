import { useId } from 'react';
import type { Movie } from '../types/movie';
import { MovieCard } from './MovieCard';

interface MovieRowProps {
  title: string;
  movies: Movie[];
  onSelect: (movie: Movie) => void;
}

export function MovieRow({ title, movies, onSelect }: MovieRowProps) {
  const headingId = useId();

  return (
    <section aria-labelledby={headingId}>
      <h2 id={headingId} className="mb-3 px-4 text-lg font-semibold text-neutral-100">
        {title}
      </h2>
      <ul className="scrollbar-none flex gap-3 overflow-x-auto px-4 pb-2">
        {movies.map((movie) => (
          <li key={movie.id} className="w-36 shrink-0 sm:w-40 lg:w-44">
            <MovieCard movie={movie} onSelect={onSelect} />
          </li>
        ))}
      </ul>
    </section>
  );
}
