import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MovieRow } from '../components/MovieRow';
import { createMovie } from './testUtils';

describe('MovieRow', () => {
  const movies = [
    createMovie({ id: 'a', title: 'The Matrix' }),
    createMovie({ id: 'b', title: 'Alien' }),
  ];

  it('renders the row heading', () => {
    render(<MovieRow title="1999" movies={movies} onSelect={vi.fn()} />);

    expect(screen.getByRole('heading', { name: '1999' })).toBeInTheDocument();
  });

  it('renders one card per movie', () => {
    render(<MovieRow title="1999" movies={movies} onSelect={vi.fn()} />);

    expect(screen.getAllByRole('button')).toHaveLength(2);
    expect(screen.getByText('The Matrix')).toBeInTheDocument();
    expect(screen.getByText('Alien')).toBeInTheDocument();
  });

  it('selects a movie from the row', async () => {
    const user = userEvent.setup();
    const onSelect = vi.fn();

    render(<MovieRow title="1999" movies={movies} onSelect={onSelect} />);
    await user.click(screen.getByRole('button', { name: /Alien/ }));

    expect(onSelect).toHaveBeenCalledWith(movies[1]);
  });
});
