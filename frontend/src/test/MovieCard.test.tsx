import { fireEvent, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MovieCard } from '../components/MovieCard';
import { createMovie } from './testUtils';

describe('MovieCard', () => {
  it('renders the poster with a descriptive alternative text', () => {
    const movie = createMovie({ title: 'The Matrix' });

    render(<MovieCard movie={movie} onSelect={vi.fn()} />);

    expect(screen.getByAltText('Poster of The Matrix')).toHaveAttribute('src', movie.posterUrl);
  });

  it('exposes an accessible label with the title and year', () => {
    render(<MovieCard movie={createMovie({ title: 'The Matrix', year: 1999 })} onSelect={vi.fn()} />);

    expect(screen.getByRole('button', { name: 'Show details for The Matrix (1999)' })).toBeInTheDocument();
  });

  it('selects the movie when clicked', async () => {
    const user = userEvent.setup();
    const onSelect = vi.fn();
    const movie = createMovie();

    render(<MovieCard movie={movie} onSelect={onSelect} />);
    await user.click(screen.getByRole('button'));

    expect(onSelect).toHaveBeenCalledWith(movie);
  });

  it('shows a fallback when the poster fails to load', () => {
    render(<MovieCard movie={createMovie()} onSelect={vi.fn()} />);

    fireEvent.error(screen.getByRole('img'));

    expect(screen.getByText('No poster')).toBeInTheDocument();
  });
});
