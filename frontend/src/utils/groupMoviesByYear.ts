import type { Movie, MovieYearGroup } from '../types/movie';

export function groupMoviesByYear(movies: Movie[]): MovieYearGroup[] {
  const buckets = new Map<number, Movie[]>();

  for (const movie of movies) {
    const bucket = buckets.get(movie.year);

    if (bucket === undefined) {
      buckets.set(movie.year, [movie]);
    } else {
      bucket.push(movie);
    }
  }

  return [...buckets.entries()]
    .sort(([leftYear], [rightYear]) => rightYear - leftYear)
    .map(([year, yearMovies]) => ({ year, movies: yearMovies }));
}
