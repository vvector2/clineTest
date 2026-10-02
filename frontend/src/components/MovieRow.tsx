import { useCallback, useEffect, useId, useRef, useState } from 'react';
import type { Movie } from '../types/movie';
import { ChevronIcon } from './icons';
import { MovieCard } from './MovieCard';

interface MovieRowProps {
  title: string;
  movies: Movie[];
  onSelect: (movie: Movie) => void;
}

const SCROLL_STEP_RATIO = 0.9;
const SCROLL_TOLERANCE_PX = 1;

export function MovieRow({ title, movies, onSelect }: MovieRowProps) {
  const headingId = useId();
  const trackRef = useRef<HTMLUListElement>(null);
  const [canScrollLeft, setCanScrollLeft] = useState(false);
  const [canScrollRight, setCanScrollRight] = useState(false);

  const syncScrollState = useCallback(() => {
    const track = trackRef.current;

    if (track === null) {
      return;
    }

    const maxScrollLeft = track.scrollWidth - track.clientWidth;
    setCanScrollLeft(track.scrollLeft > SCROLL_TOLERANCE_PX);
    setCanScrollRight(track.scrollLeft < maxScrollLeft - SCROLL_TOLERANCE_PX);
  }, []);

  const scrollTrack = (direction: -1 | 1) => {
    const track = trackRef.current;

    if (track === null) {
      return;
    }

    track.scrollBy({
      left: direction * track.clientWidth * SCROLL_STEP_RATIO,
      behavior: 'smooth',
    });
  };

  useEffect(() => {
    syncScrollState();

    const track = trackRef.current;

    if (track === null) {
      return;
    }

    const observer = new ResizeObserver(syncScrollState);
    observer.observe(track);

    return () => observer.disconnect();
  }, [syncScrollState, movies]);

  return (
    <section aria-labelledby={headingId} className="group/row relative">
      <h2 id={headingId} className="mb-3 px-4 text-lg font-semibold text-neutral-100">
        {title}
      </h2>
      <div className="relative">
        <ul
          ref={trackRef}
          onScroll={syncScrollState}
          className="scrollbar-none flex gap-3 overflow-x-auto px-4 pb-2"
        >
          {movies.map((movie) => (
            <li key={movie.id} className="w-36 shrink-0 sm:w-40 lg:w-44">
              <MovieCard movie={movie} onSelect={onSelect} />
            </li>
          ))}
        </ul>
        <button
          type="button"
          onClick={() => scrollTrack(-1)}
          disabled={!canScrollLeft}
          aria-label={`Scroll ${title} to the left`}
          className="absolute top-1/2 left-1 z-10 -translate-y-1/2 rounded-full bg-neutral-950/80 p-2 text-neutral-100 opacity-0 transition group-hover/row:opacity-100 focus-visible:opacity-100 hover:bg-neutral-950 disabled:pointer-events-none disabled:opacity-0 focus-visible:ring-2 focus-visible:ring-red-600 focus-visible:outline-none"
        >
          <ChevronIcon className="size-5" direction="left" />
        </button>
        <button
          type="button"
          onClick={() => scrollTrack(1)}
          disabled={!canScrollRight}
          aria-label={`Scroll ${title} to the right`}
          className="absolute top-1/2 right-1 z-10 -translate-y-1/2 rounded-full bg-neutral-950/80 p-2 text-neutral-100 opacity-0 transition group-hover/row:opacity-100 focus-visible:opacity-100 hover:bg-neutral-950 disabled:pointer-events-none disabled:opacity-0 focus-visible:ring-2 focus-visible:ring-red-600 focus-visible:outline-none"
        >
          <ChevronIcon className="size-5" direction="right" />
        </button>
      </div>
    </section>
  );
}
