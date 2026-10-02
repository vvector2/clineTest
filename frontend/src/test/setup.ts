import * as jestDomMatchers from '@testing-library/jest-dom/matchers';
import type {} from '@testing-library/jest-dom/vitest';

expect.extend(jestDomMatchers);

class ResizeObserverStub implements ResizeObserver {
  constructor(private readonly callback: ResizeObserverCallback) {}

  observe(): void {
    this.callback([], this);
  }

  unobserve(): void {}

  disconnect(): void {}
}

globalThis.ResizeObserver = ResizeObserverStub;
