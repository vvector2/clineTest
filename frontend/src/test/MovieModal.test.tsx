import { fireEvent, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MovieModal } from '../components/MovieModal';
import { createMovie } from './testUtils';

describe('MovieModal', () => {
  const movie = createMovie({ title: 'The Matrix', year: 1999 });

  it('renders nothing when no movie is selected', () => {
    const { container } = render(<MovieModal movie={null} onClose={vi.fn()} />);

    expect(container).toBeEmptyDOMElement();
  });

  it('renders the movie details inside a modal dialog', () => {
    render(<MovieModal movie={movie} onClose={vi.fn()} />);

    const dialog = screen.getByRole('dialog', { name: 'The Matrix' });

    expect(dialog).toBeInTheDocument();
    expect(screen.getByText('Neo learns what the Matrix really is.')).toBeInTheDocument();
    expect(screen.getByText(/Lana Wachowski/)).toBeInTheDocument();
  });

  it('moves focus to the close button when it opens', () => {
    render(<MovieModal movie={movie} onClose={vi.fn()} />);

    expect(screen.getByRole('button', { name: 'Close details' })).toHaveFocus();
  });

  it('closes when the close button is pressed', async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();

    render(<MovieModal movie={movie} onClose={onClose} />);
    await user.click(screen.getByRole('button', { name: 'Close details' }));

    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it('closes when the backdrop is clicked', async () => {
    const user = userEvent.setup();
    const onClose = vi.fn();
    const { container } = render(<MovieModal movie={movie} onClose={onClose} />);

    const backdrop = container.querySelector('button[aria-hidden="true"]');
    expect(backdrop).not.toBeNull();

    await user.click(backdrop as HTMLButtonElement);

    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it('closes when the escape key is pressed', () => {
    const onClose = vi.fn();

    render(<MovieModal movie={movie} onClose={onClose} />);
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));

    expect(onClose).toHaveBeenCalledTimes(1);
  });

  it('ignores other keys', () => {
    const onClose = vi.fn();

    render(<MovieModal movie={movie} onClose={onClose} />);
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'a' }));

    expect(onClose).not.toHaveBeenCalled();
  });

  it('falls back to a placeholder when the poster cannot be loaded', () => {
    const { container } = render(<MovieModal movie={movie} onClose={vi.fn()} />);

    fireEvent.error(screen.getByAltText('Poster of The Matrix'));

    expect(screen.queryByAltText('Poster of The Matrix')).not.toBeInTheDocument();
    expect(container.querySelector('svg')).not.toBeNull();
  });
});
