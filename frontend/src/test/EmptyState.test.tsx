import { render, screen } from '@testing-library/react';
import { EmptyState } from '../components/EmptyState';

describe('EmptyState', () => {
  it('explains that nothing matched the search phrase', () => {
    render(<EmptyState searchPhrase="nothing here" />);

    expect(screen.getByRole('heading', { name: 'No movies found' })).toBeInTheDocument();
    expect(screen.getByText('nothing here')).toBeInTheDocument();
  });

  it('ignores a blank search phrase', () => {
    render(<EmptyState searchPhrase="   " />);

    expect(screen.getByRole('heading', { name: 'No movies found' })).toBeInTheDocument();
    expect(screen.queryByText('Nothing matched')).not.toBeInTheDocument();
  });
});
