import { useCallback } from 'react';

interface ErrorMessageProps {
  message: string;
  onRetry: () => void;
}

export function ErrorMessage({ message, onRetry }: ErrorMessageProps) {
  const handleRetry = useCallback(() => {
    onRetry();
  }, [onRetry]);

  return (
    <div
      role="alert"
      className="mx-auto flex max-w-md flex-col items-center gap-3 rounded-lg border border-red-900/60 bg-red-950/40 p-6 text-center"
    >
      <h2 className="text-lg font-semibold text-red-200">Something went wrong</h2>
      <p className="text-sm text-red-100/80">{message}</p>
      <button
        type="button"
        onClick={handleRetry}
        className="rounded bg-red-600 px-4 py-2 text-sm font-semibold text-white transition-colors hover:bg-red-700 focus-visible:ring-2 focus-visible:ring-white focus-visible:outline-none"
      >
        Try again
      </button>
    </div>
  );
}
