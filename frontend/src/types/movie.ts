export interface Movie {
  id: string;
  title: string;
  year: number;
  description: string;
  director: string;
  overview: string;
  originalLanguage: string;
  posterUrl: string;
  genres?: string[];
}

export interface MovieYearGroup {
  year: number;
  movies: Movie[];
}
