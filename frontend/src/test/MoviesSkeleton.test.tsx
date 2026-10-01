import { render, screen } from '@testing-library/react';
import { MoviesSkeleton } from '../components/MoviesSkeleton';

describe('MoviesSkeleton', () => {
  it('exposes a loading status for screen readers', () => {
    render(<MoviesSkeleton />);

    expect(screen.getByRole('status')).toHaveTextContent('Loading movies...');
  });
});
