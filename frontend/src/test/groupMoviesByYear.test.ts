import { groupMoviesByYear } from '../utils/groupMoviesByYear';
import { createMovie } from './testUtils';

describe('groupMoviesByYear', () => {
  it('returns an empty array when there are no movies', () => {
    expect(groupMoviesByYear([])).toEqual([]);
  });

  it('groups movies that share the same release year', () => {
    const movies = [
      createMovie({ id: 'a', year: 1999 }),
      createMovie({ id: 'b', year: 2020 }),
      createMovie({ id: 'c', year: 1999 }),
    ];

    const groups = groupMoviesByYear(movies);

    expect(groups).toHaveLength(2);
    expect(groups.map((group) => group.year)).toEqual([2020, 1999]);
    expect(groups[1].movies.map((movie) => movie.id)).toEqual(['a', 'c']);
  });

  it('sorts the groups from the most recent year to the oldest', () => {
    const movies = [
      createMovie({ id: 'a', year: 1994 }),
      createMovie({ id: 'b', year: 2024 }),
      createMovie({ id: 'c', year: 2010 }),
    ];

    expect(groupMoviesByYear(movies).map((group) => group.year)).toEqual([2024, 2010, 1994]);
  });
});
