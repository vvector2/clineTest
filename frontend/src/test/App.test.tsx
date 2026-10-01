import { render, screen, waitFor, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { App } from '../App';
import { createJsonResponse, createMovie, fetchMock, stubFetch } from './testUtils';

const waitForOptions = { timeout: 3000 };

describe('App', () => {
  const movies = [
    createMovie({ id: 'a', title: 'The Matrix', year: 1999 }),
    createMovie({ id: 'b', title: 'Arrival', year: 2016 }),
  ];

  beforeEach(() => {
    stubFetch();
  });

  afterEach(() => {
    fetchMock.mockReset();
  });

  it('renders the brand and the search bar', async () => {
    fetchMock.mockResolvedValue(createJsonResponse([]));

    render(<App />);

    expect(screen.getByText('ClineFlix')).toBeInTheDocument();
    expect(screen.getByRole('searchbox')).toBeInTheDocument();

    await waitFor(() => expect(fetchMock).toHaveBeenCalled(), waitForOptions);
  });

  it('loads the catalogue and groups it by release year', async () => {
    fetchMock.mockResolvedValue(createJsonResponse(movies));

    render(<App />);

    await waitFor(() => expect(screen.getByRole('heading', { name: '2016' })).toBeInTheDocument(), waitForOptions);

    expect(screen.getByRole('heading', { name: '1999' })).toBeInTheDocument();
    expect(screen.getByAltText('Poster of The Matrix')).toBeInTheDocument();
    expect(fetchMock).toHaveBeenCalledWith('/api/movies', expect.any(Object));
  });

  it('filters the catalogue through the search endpoint', async () => {
    const user = userEvent.setup();
    fetchMock.mockResolvedValue(createJsonResponse(movies));

    render(<App />);
    await waitFor(() => expect(screen.getByRole('heading', { name: '2016' })).toBeInTheDocument(), waitForOptions);

    await user.type(screen.getByRole('searchbox'), 'matrix');

    await waitFor(
      () => expect(fetchMock).toHaveBeenCalledWith('/api/movies?query=matrix', expect.any(Object)),
      waitForOptions,
    );
  });

  it('opens the details dialog when a poster is clicked', async () => {
    const user = userEvent.setup();
    fetchMock.mockResolvedValue(createJsonResponse(movies));

    render(<App />);
    await waitFor(() => expect(screen.getByAltText('Poster of The Matrix')).toBeInTheDocument(), waitForOptions);

    await user.click(screen.getByRole('button', { name: 'Show details for The Matrix (1999)' }));

    const dialog = await screen.findByRole('dialog', { name: 'The Matrix' });

    expect(within(dialog).getByText('Neo learns what the Matrix really is.')).toBeInTheDocument();

    await user.click(within(dialog).getByRole('button', { name: 'Close details' }));

    await waitFor(() => expect(screen.queryByRole('dialog')).not.toBeInTheDocument());
  });

  it('shows a retryable error when the catalogue cannot be loaded', async () => {
    const user = userEvent.setup();
    fetchMock.mockResolvedValue(createJsonResponse([], false, 500));

    render(<App />);

    await waitFor(() => expect(screen.getByRole('alert')).toBeInTheDocument(), waitForOptions);
    expect(screen.getByRole('alert')).toHaveTextContent('status 500');

    fetchMock.mockResolvedValue(createJsonResponse(movies));
    await user.click(screen.getByRole('button', { name: 'Try again' }));

    await waitFor(() => expect(screen.getByRole('heading', { name: '2016' })).toBeInTheDocument(), waitForOptions);
  });
});
