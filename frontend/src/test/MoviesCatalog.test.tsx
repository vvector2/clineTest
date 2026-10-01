import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MoviesCatalog } from '../components/MoviesCatalog';
import { createMovie } from './testUtils';

const defaultProps = {
  isLoading: false,
  error: null,
  searchPhrase: '',
  onRetry: vi.fn(),
  onSelect: vi.fn(),
};

describe('MoviesCatalog', () => {
  it('shows the loading skeleton while the request is in flight', () => {
    render(<MoviesCatalog {...defaultProps} movies={[]} isLoading />);

    expect(screen.getByRole('status')).toBeInTheDocument();
    expect(screen.queryByRole('alert')).not.toBeInTheDocument();
  });

  it('shows an error with a retry action', () => {
    render(<MoviesCatalog {...defaultProps} movies={[]} error="Boom." />);

    expect(screen.getByRole('alert')).toHaveTextContent('Boom.');
  });

  it('shows the empty state when there are no movies', () => {
    render(<MoviesCatalog {...defaultProps} movies={[]} />);

    expect(screen.getByRole('heading', { name: 'No movies found' })).toBeInTheDocument();
  });

  it('groups the catalogue into rows by release year', () => {
    const movies = [
      createMovie({ id: 'a', title: 'The Matrix', year: 1999 }),
      createMovie({ id: 'b', title: 'Arrival', year: 2016 }),
      createMovie({ id: 'c', title: 'Heat', year: 1995 }),
    ];

    render(<MoviesCatalog {...defaultProps} movies={movies} />);

    expect(screen.getByRole('heading', { name: '2016' })).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: '1999' })).toBeInTheDocument();
    expect(screen.getByRole('heading', { name: '1995' })).toBeInTheDocument();
  });

  it('switches to a result grid when searching', () => {
    const movies = [createMovie({ id: 'a', title: 'The Matrix' })];

    render(<MoviesCatalog {...defaultProps} movies={movies} searchPhrase="matrix" />);

    expect(screen.getByRole('heading', { name: /1 result for/i })).toBeInTheDocument();
    expect(screen.queryByRole('heading', { name: '1999' })).not.toBeInTheDocument();
  });

  it('uses the plural form for several results', () => {
    const movies = [createMovie({ id: 'a' }), createMovie({ id: 'b' })];

    render(<MoviesCatalog {...defaultProps} movies={movies} searchPhrase="matrix" />);

    expect(screen.getByRole('heading', { name: /2 results for/i })).toBeInTheDocument();
  });

  it('selects a movie from the grid', async () => {
    const user = userEvent.setup();
    const onSelect = vi.fn();
    const movies = [createMovie({ id: 'a', title: 'The Matrix' })];

    render(<MoviesCatalog {...defaultProps} movies={movies} searchPhrase="matrix" onSelect={onSelect} />);
    await user.click(screen.getByRole('button', { name: /The Matrix/ }));

    expect(onSelect).toHaveBeenCalledWith(movies[0]);
  });

  it('retries from the error state', async () => {
    const user = userEvent.setup();
    const onRetry = vi.fn();

    render(<MoviesCatalog {...defaultProps} movies={[]} error="Boom." onRetry={onRetry} />);
    await user.click(screen.getByRole('button', { name: 'Try again' }));

    expect(onRetry).toHaveBeenCalledTimes(1);
  });
});
