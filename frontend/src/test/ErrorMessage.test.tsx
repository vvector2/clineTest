import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ErrorMessage } from '../components/ErrorMessage';

describe('ErrorMessage', () => {
  it('announces the failure to assistive technology', () => {
    render(<ErrorMessage message="The movie service responded with status 500." onRetry={vi.fn()} />);

    expect(screen.getByRole('alert')).toHaveTextContent('The movie service responded with status 500.');
  });

  it('retries when the try again button is pressed', async () => {
    const user = userEvent.setup();
    const onRetry = vi.fn();

    render(<ErrorMessage message="Boom." onRetry={onRetry} />);
    await user.click(screen.getByRole('button', { name: 'Try again' }));

    expect(onRetry).toHaveBeenCalledTimes(1);
  });
});
