import { ClineLogo } from './icons';
import { SearchBar } from './SearchBar';

interface HeaderProps {
  searchPhrase: string;
  onSearchPhraseChange: (value: string) => void;
}

export function Header({ searchPhrase, onSearchPhraseChange }: HeaderProps) {
  return (
    <header className="sticky top-0 z-30 border-b border-neutral-800 bg-neutral-950/90 backdrop-blur-md">
      <div className="mx-auto flex max-w-7xl items-center justify-between gap-4 px-4 py-3 sm:px-6 lg:px-8">
        <p className="flex items-center gap-2 text-xl font-black tracking-tight text-red-600 sm:text-2xl">
          <ClineLogo className="size-6 shrink-0 sm:size-8" />
          ClineFlix
        </p>
        <SearchBar value={searchPhrase} onChange={onSearchPhraseChange} />
      </div>
    </header>
  );
}
