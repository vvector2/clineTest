import { useCallback, useState } from 'react';
import { Header } from './components/Header';
import { MovieModal } from './components/MovieModal';
import { MoviesCatalog } from './components/MoviesCatalog';
import { useMovies } from './hooks/useMovies';
import type { Movie } from './types/movie';

export function App() {
  const [searchPhrase, setSearchPhrase] = useState('');
  const [selectedMovie, setSelectedMovie] = useState<Movie | null>(null);
  const { movies, isLoading, error, reload } = useMovies(searchPhrase);

  const handleSearchPhraseChange = useCallback((value: string) => {
    setSearchPhrase(value);
  }, []);

  const handleSelectMovie = useCallback((movie: Movie) => {
    setSelectedMovie(movie);
  }, []);

  const handleCloseModal = useCallback(() => {
    setSelectedMovie(null);
  }, []);

  return (
    <div className="flex min-h-screen flex-col bg-neutral-950 text-neutral-100">
      <Header searchPhrase={searchPhrase} onSearchPhraseChange={handleSearchPhraseChange} />
      <main className="mx-auto w-full max-w-7xl flex-1 px-0 py-8 sm:px-6 lg:px-8">
        <MoviesCatalog
          movies={movies}
          isLoading={isLoading}
          error={error}
          searchPhrase={searchPhrase}
          onRetry={reload}
          onSelect={handleSelectMovie}
        />
      </main>
      <MovieModal movie={selectedMovie} onClose={handleCloseModal} />
    </div>
  );
}
