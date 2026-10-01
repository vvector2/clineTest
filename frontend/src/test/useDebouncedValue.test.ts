import { act, renderHook } from '@testing-library/react';
import { useDebouncedValue } from '../hooks/useDebouncedValue';

describe('useDebouncedValue', () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('returns the initial value on the first render', () => {
    const { result } = renderHook(() => useDebouncedValue('matrix', 300));

    expect(result.current).toBe('matrix');
  });

  it('keeps the previous value until the delay has passed', () => {
    const { result, rerender } = renderHook(({ value }) => useDebouncedValue(value, 300), {
      initialProps: { value: 'matrix' },
    });

    rerender({ value: 'alien' });

    expect(result.current).toBe('matrix');

    act(() => {
      vi.advanceTimersByTime(299);
    });

    expect(result.current).toBe('matrix');

    act(() => {
      vi.advanceTimersByTime(1);
    });

    expect(result.current).toBe('alien');
  });

  it('settles on the latest value when the input changes repeatedly', () => {
    const { result, rerender } = renderHook(({ value }) => useDebouncedValue(value, 300), {
      initialProps: { value: 'a' },
    });

    rerender({ value: 'ab' });
    act(() => {
      vi.advanceTimersByTime(200);
    });

    rerender({ value: 'abc' });
    act(() => {
      vi.advanceTimersByTime(300);
    });

    expect(result.current).toBe('abc');
  });
});
