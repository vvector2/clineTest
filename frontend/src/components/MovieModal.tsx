import { useCallback, useEffect, useId, useRef, useState } from 'react';
import type { Movie } from '../types/movie';
import { CloseIcon, PosterFallbackIcon } from './icons';

interface MovieModalProps {
  movie: Movie | null;
  onClose: () => void;
}

export function MovieModal({ movie, onClose }: MovieModalProps) {
  const closeButtonRef = useRef<HTMLButtonElement>(null);
  const headingId = useId();
  const [hasPosterError, setHasPosterError] = useState(false);
  const isOpen = movie !== null;

  const handleClose = useCallback(() => {
    onClose();
  }, [onClose]);

  const handleImageError = useCallback(() => {
    setHasPosterError(true);
  }, []);

  useEffect(() => {
    if (!isOpen) {
      return;
    }

    closeButtonRef.current?.focus();

    const handleKeyDown = (event: KeyboardEvent): void => {
      if (event.key === 'Escape') {
        handleClose();
      }
    };

    document.addEventListener('keydown', handleKeyDown);

    return () => {
      document.removeEventListener('keydown', handleKeyDown);
    };
  }, [isOpen, handleClose]);

  if (movie === null) {
    return null;
  }

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby={headingId}
      className="fixed inset-0 z-50 flex items-center justify-center overflow-y-auto bg-black/80 p-4"
    >
      <button
        type="button"
        aria-hidden="true"
        tabIndex={-1}
        onClick={handleClose}
        className="absolute inset-0 h-full w-full cursor-default"
      />
      <article className="relative z-10 w-full max-w-3xl overflow-hidden rounded-lg bg-neutral-900 shadow-2xl ring-1 ring-neutral-700">
        <button
          ref={closeButtonRef}
          type="button"
          onClick={handleClose}
          aria-label="Close details"
          className="absolute top-3 right-3 z-20 rounded-full bg-black/70 p-2 text-white transition-colors hover:bg-black focus-visible:ring-2 focus-visible:ring-red-600 focus-visible:outline-none"
        >
          <CloseIcon />
        </button>
        <div className="flex max-h-80 items-center justify-center bg-neutral-800">
          {hasPosterError ? (
            <PosterFallbackIcon className="my-10 size-16 text-neutral-600" />
          ) : (
            <img
              src={movie.posterUrl}
              alt={`Poster of ${movie.title}`}
              onError={handleImageError}
              className="max-h-80 w-full object-cover"
            />
          )}
        </div>
        <div className="space-y-4 p-6">
          <header>
            <h2 id={headingId} className="text-2xl font-bold text-white">
              {movie.title}
            </h2>
            <p className="mt-1 text-sm text-neutral-400">
              {movie.year} &middot; {movie.originalLanguage} &middot; {movie.director}
            </p>
          </header>
          <p className="text-sm leading-relaxed text-neutral-200">{movie.overview}</p>
          <p className="text-sm leading-relaxed text-neutral-400">{movie.description}</p>
        </div>
      </article>
    </div>
  );
}
