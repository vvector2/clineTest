import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Header } from '../components/Header';

describe('Header', () => {
  it('renders the brand and the search bar', () => {
    render(<Header searchPhrase="" onSearchPhraseChange={vi.fn()} />);

    expect(screen.getByText('ClineFlix')).toBeInTheDocument();
    expect(screen.getByRole('searchbox')).toBeInTheDocument();
  });

  it('shows the current search phrase', () => {
    render(<Header searchPhrase="matrix" onSearchPhraseChange={vi.fn()} />);

    expect(screen.getByRole('searchbox')).toHaveValue('matrix');
  });

  it('propagates a new search phrase', async () => {
    const user = userEvent.setup();
    const onSearchPhraseChange = vi.fn();
    render(<Header searchPhrase="" onSearchPhraseChange={onSearchPhraseChange} />);

    await user.type(screen.getByRole('searchbox'), 'a');

    expect(onSearchPhraseChange).toHaveBeenCalledWith('a');
  });
});
