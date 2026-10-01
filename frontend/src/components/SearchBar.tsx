import { useCallback, type ChangeEvent } from 'react';
import { CloseIcon, SearchIcon } from './icons';

interface SearchBarProps {
  value: string;
  onChange: (value: string) => void;
}

export function SearchBar({ value, onChange }: SearchBarProps) {
  const handleChange = useCallback(
    (event: ChangeEvent<HTMLInputElement>) => {
      onChange(event.target.value);
    },
    [onChange],
  );

  const handleClear = useCallback(() => {
    onChange('');
  }, [onChange]);

  const hasValue = value.length > 0;

  return (
    <div role="search" className="relative w-full sm:w-72">
      <label htmlFor="movie-search" className="sr-only">
        Search movies by title, director or overview
      </label>
      <SearchIcon className="pointer-events-none absolute top-1/2 left-3 size-4 -translate-y-1/2 text-neutral-400" />
      <input
        id="movie-search"
        type="search"
        value={value}
        onChange={handleChange}
        placeholder="Search movies..."
        autoComplete="off"
        className="w-full rounded-md border border-neutral-700 bg-neutral-900/80 py-2 pr-10 pl-10 text-sm text-neutral-100 placeholder:text-neutral-500 focus-visible:border-red-600 focus-visible:ring-2 focus-visible:ring-red-600 focus-visible:outline-none"
      />
      {hasValue && (
        <button
          type="button"
          onClick={handleClear}
          aria-label="Clear search"
          className="absolute top-1/2 right-2 -translate-y-1/2 rounded p-1 text-neutral-400 transition-colors hover:text-neutral-100 focus-visible:ring-2 focus-visible:ring-red-600 focus-visible:outline-none"
        >
          <CloseIcon />
        </button>
      )}
    </div>
  );
}
