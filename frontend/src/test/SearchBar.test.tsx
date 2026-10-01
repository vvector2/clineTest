import { useState } from 'react';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { SearchBar } from '../components/SearchBar';

function ControlledSearchBar({
  initialValue = '',
  onChange,
}: {
  initialValue?: string;
  onChange?: (value: string) => void;
}) {
  const [value, setValue] = useState(initialValue);

  return (
    <SearchBar
      value={value}
      onChange={(nextValue) => {
        setValue(nextValue);
        onChange?.(nextValue);
      }}
    />
  );
}

describe('SearchBar', () => {
  it('renders an accessible search field', () => {
    render(<SearchBar value="" onChange={vi.fn()} />);

    expect(screen.getByRole('search')).toBeInTheDocument();
    expect(screen.getByRole('searchbox', { name: /search movies by title, director or overview/i })).toBeInTheDocument();
  });

  it('reports every keystroke to the parent', async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    render(<ControlledSearchBar onChange={onChange} />);

    await user.type(screen.getByRole('searchbox'), 'matrix');

    expect(onChange).toHaveBeenCalledWith('m');
    expect(onChange).toHaveBeenLastCalledWith('matrix');
  });

  it('updates the displayed value when typing', async () => {
    const user = userEvent.setup();
    render(<ControlledSearchBar />);
    const input = screen.getByRole('searchbox');

    await user.type(input, 'alien');

    expect(input).toHaveValue('alien');
  });

  it('hides the clear button while the search box is empty', () => {
    render(<SearchBar value="" onChange={vi.fn()} />);

    expect(screen.queryByRole('button', { name: 'Clear search' })).not.toBeInTheDocument();
  });

  it('clears the search phrase when the clear button is pressed', async () => {
    const user = userEvent.setup();
    render(<ControlledSearchBar initialValue="alien" />);

    await user.click(screen.getByRole('button', { name: 'Clear search' }));

    expect(screen.getByRole('searchbox')).toHaveValue('');
    expect(screen.queryByRole('button', { name: 'Clear search' })).not.toBeInTheDocument();
  });
});
