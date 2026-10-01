# Frontend Development Rules

## Tech Stack & Architecture

- **Framework & Language**: Use TypeScript with React (Next.js App Router preferred where applicable).
- **Styling**: Use Tailwind CSS for component styling. Prefer standard utility classes over inline styles or raw CSS files.
- **Component Design**: Build modular, single-responsibility components. Keep components under 150 lines where possible; break larger UI into smaller sub-components.

## TypeScript Standards

- Enable strict mode (`strict: true`).
- Avoid `any` at all costs. Define precise interfaces or type aliases for component props, API payloads, and state models.
- Export interfaces alongside component exports when they are shared across files.

## Component & State Management

- Prefer Functional Components with React Hooks.
- Keep state as local as possible. Do not lift state higher than necessary or use global store (Zustand/Redux) for pure local UI toggles.
- Use `useCallback` and `useMemo` deliberately to optimize performance, especially for heavy calculations or functions passed down to memoized children.

## Code Style & Readability

- Follow standard naming conventions:
  - Component files: `PascalCase.tsx` (e.g., `UserProfileCard.tsx`)
  - Utilities/hooks: `camelCase.ts` (e.g., `useAuth.ts`, `formatCurrency.ts`)
  - CSS/Tailwind classes: Follow standard class order (layout -> spacing -> typography -> visual effects).
- Prefer named exports over default exports for clarity and reliable auto-imports.
- Always include explicit `alt` attributes for images and accessible labels for non-text interactive elements (`aria-label`).

## Accessibility (a11y) & UX

- Ensure semantic HTML tags (`<header>`, `<main>`, `<nav>`, `<button>`, `<article>`) are used instead of generic `<div>` wrappers.
- All interactive elements must be keyboard accessible and display a visible focus indicator (`focus-visible:outline-*`).
- Provide loading skeletons or clear spinner indicators for asynchronous visual transitions.

## Error Handling & API Integration

- Wrap API calls in standard `try/catch` blocks or use robust query libraries (e.g., TanStack Query).
- Display user-friendly error UI or toast notifications instead of failing silently or crashing the application.
