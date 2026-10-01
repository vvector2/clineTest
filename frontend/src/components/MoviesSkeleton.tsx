const SKELETON_ROWS = 3;
const SKELETON_CARDS_PER_ROW = 8;

function SkeletonCards() {
  return (
    <>
      {Array.from({ length: SKELETON_CARDS_PER_ROW }, (_, index) => (
        <li key={index} className="w-36 shrink-0 sm:w-40 lg:w-44">
          <div className="aspect-2/3 w-full animate-pulse rounded-md bg-neutral-800" />
        </li>
      ))}
    </>
  );
}

export function MoviesSkeleton() {
  return (
    <div className="space-y-10" role="status" aria-live="polite">
      <span className="sr-only">Loading movies...</span>
      {Array.from({ length: SKELETON_ROWS }, (_, rowIndex) => (
        <section key={rowIndex} aria-hidden="true">
          <div className="mx-4 mb-3 h-5 w-32 animate-pulse rounded bg-neutral-800" />
          <ul className="flex gap-3 overflow-hidden px-4">
            <SkeletonCards />
          </ul>
        </section>
      ))}
    </div>
  );
}
