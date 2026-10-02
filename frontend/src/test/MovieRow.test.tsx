import { fireEvent, render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { MovieRow } from '../components/MovieRow';
import { createMovie } from './testUtils';

const TRACK_WIDTH = 500;
const OVERFLOWING_WIDTH = 900;

function setTrackMetrics(
  track: HTMLElement,
  { clientWidth, scrollWidth, scrollLeft = 0 }: { clientWidth: number; scrollWidth: number; scrollLeft?: number },
): void {
  Object.defineProperty(track, 'clientWidth', { configurable: true, value: clientWidth });
  Object.defineProperty(track, 'scrollWidth', { configurable: true, value: scrollWidth });
  Object.defineProperty(track, 'scrollLeft', { configurable: true, writable: true, value: scrollLeft });
}

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

    expect(within(screen.getByRole('list')).getAllByRole('button')).toHaveLength(2);
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

  describe('scrolling', () => {
    it('enables the next control only when the row overflows', () => {
      render(<MovieRow title="1999" movies={movies} onSelect={vi.fn()} />);

      const track = screen.getByRole('list');
      setTrackMetrics(track, { clientWidth: TRACK_WIDTH, scrollWidth: OVERFLOWING_WIDTH });
      fireEvent.scroll(track);

      expect(screen.getByRole('button', { name: 'Scroll 1999 to the left' })).toBeDisabled();
      expect(screen.getByRole('button', { name: 'Scroll 1999 to the right' })).toBeEnabled();
    });

    it('disables both controls when every poster fits on screen', () => {
      render(<MovieRow title="1999" movies={movies} onSelect={vi.fn()} />);

      const track = screen.getByRole('list');
      setTrackMetrics(track, { clientWidth: OVERFLOWING_WIDTH, scrollWidth: OVERFLOWING_WIDTH });
      fireEvent.scroll(track);

      expect(screen.getByRole('button', { name: 'Scroll 1999 to the left' })).toBeDisabled();
      expect(screen.getByRole('button', { name: 'Scroll 1999 to the right' })).toBeDisabled();
    });

    it('disables the previous control once the row is scrolled to the start', () => {
      render(<MovieRow title="1999" movies={movies} onSelect={vi.fn()} />);

      const track = screen.getByRole('list');
      setTrackMetrics(track, {
        clientWidth: TRACK_WIDTH,
        scrollWidth: OVERFLOWING_WIDTH,
        scrollLeft: TRACK_WIDTH,
      });
      fireEvent.scroll(track);

      expect(screen.getByRole('button', { name: 'Scroll 1999 to the left' })).toBeEnabled();
      expect(screen.getByRole('button', { name: 'Scroll 1999 to the right' })).toBeDisabled();
    });

    it('pages the row forwards and backwards', async () => {
      const user = userEvent.setup();
      render(<MovieRow title="1999" movies={movies} onSelect={vi.fn()} />);

      const track = screen.getByRole('list');
      setTrackMetrics(track, { clientWidth: TRACK_WIDTH, scrollWidth: OVERFLOWING_WIDTH });
      const scrollBy = vi.fn();
      track.scrollBy = scrollBy;

      fireEvent.scroll(track);
      await user.click(screen.getByRole('button', { name: 'Scroll 1999 to the right' }));
      expect(scrollBy).toHaveBeenCalledWith({ left: 450, behavior: 'smooth' });

      setTrackMetrics(track, {
        clientWidth: TRACK_WIDTH,
        scrollWidth: OVERFLOWING_WIDTH,
        scrollLeft: TRACK_WIDTH,
      });
      fireEvent.scroll(track);

      await user.click(screen.getByRole('button', { name: 'Scroll 1999 to the left' }));
      expect(scrollBy).toHaveBeenCalledWith({ left: -450, behavior: 'smooth' });
    });

    it('does nothing when a control is pressed without a mounted track', () => {
      const { unmount } = render(<MovieRow title="1999" movies={movies} onSelect={vi.fn()} />);

      unmount();

      expect(screen.queryByRole('button', { name: /Scroll 1999/ })).not.toBeInTheDocument();
    });
  });
});
