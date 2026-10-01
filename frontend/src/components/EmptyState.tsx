interface EmptyStateProps {
  searchPhrase: string;
}

export function EmptyState({ searchPhrase }: EmptyStateProps) {
  const phrase = searchPhrase.trim();

  return (
    <div className="mx-auto flex max-w-md flex-col items-center gap-2 p-6 text-center">
      <h2 className="text-lg font-semibold text-neutral-100">No movies found</h2>
      {phrase.length > 0 && (
        <p className="text-sm text-neutral-400">
          Nothing matched <span className="font-semibold text-neutral-200">{phrase}</span>.
        </p>
      )}
      <p className="text-sm text-neutral-500">Try a different search term or clear the search box.</p>
    </div>
  );
}
