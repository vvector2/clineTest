# ClineFlix

Single page application for browsing the movie catalogue served by the
`ClientTestApi` backend. Movies are displayed as posters arranged in Netflix
style rows, and the search box in the top right corner filters the catalogue
through the backend search endpoint.

## Stack

| Concern    | Choice                     |
| ---------- | -------------------------- |
| Build tool | Vite 8                     |
| UI         | React 19 + TypeScript 5.9  |
| Styling    | Tailwind CSS 4             |
| Tests      | Vitest + Testing Library   |
| Linting    | ESLint 9 (flat config)     |

## Getting started

```bash
npm install
npm run dev
```

The dev server listens on <http://localhost:5173>.

The API is expected on <http://localhost:5297> (`dotnet run --launch-profile http`
from `Backend/ClientTestApi`).

## Configuration

Environment variables live in `.env.development` (see `.env.example`):

| Variable                  | Default                 | Purpose                                                         |
| ------------------------- | ----------------------- | --------------------------------------------------------------- |
| `VITE_API_BASE_URL`       | *(empty)*               | Empty means "use the dev/preview proxy at `/api`".               |
| `VITE_API_PROXY_TARGET`   | `http://localhost:5297` | Backend address the Vite proxy forwards `/api/*` to.             |

### Why a proxy instead of CORS?

The browser only calls same-origin `/api/movies`, and Vite forwards that to the
backend. The request therefore looks same-origin to the browser, so **no CORS
policy is required on the backend** during development or `npm run preview`.

If the built app is deployed to a different origin than the API, set
`VITE_API_BASE_URL` to the absolute API URL — in that case the backend must
allow the frontend origin via CORS.

## Backend contract

```http
GET /movies            # full catalogue
GET /movies?query=xyz  # search by title, director or overview
```

> The query string parameter is `query` (not `quries`), matching
> `Backend/ClientTestApi/Controllers/MoviesController.cs`.

Response:

```json
[
  {
    "id": "92afe203-17fd-40b6-be0c-17ad22b3f6d0",
    "title": "The Matrix",
    "year": 1999,
    "description": "A hacker discovers the truth.",
    "director": "Lana Wachowski",
    "overview": "Neo learns what the Matrix really is.",
    "originalLanguage": "English",
    "posterUrl": "http://localhost:4566/clinetest-posters/movies/1111.jpg"
  }
]
```

## Scripts

| Script            | Description                                     |
| ----------------- | ----------------------------------------------- |
| `npm run dev`     | Start the dev server with HMR.                  |
| `npm run build`   | Type-check (`tsc -b`) and build for production. |
| `npm run preview` | Serve the production build.                     |
| `npm run test`    | Run the test suite with coverage.               |
| `npm run lint`    | Run ESLint.                                     |

## Quality gates

`npm run build && npm run test && npm run lint` must pass with no errors.
Coverage thresholds of 80% are enforced by Vitest and fail the test run.

## Project layout

```
src/
├── api/moviesApi.ts            # fetch client, runtime response validation
├── components/                 # presentational React components
├── hooks/                      # useMovies, useDebouncedValue
├── types/movie.ts              # shared Movie / MovieYearGroup types
├── utils/groupMoviesByYear.ts  # catalogue grouping
└── test/                       # Vitest suites and helpers
```

## Behaviour

- The catalogue is grouped into horizontal poster rows by release year,
  newest first.
- Typing in the search box debounces for 300 ms and then calls
  `GET /movies?query=<phrase>`; results are shown in a grid instead of rows.
- Clicking a poster opens a details dialog (closes on Escape, backdrop or the
  close button) and moves focus to the close button on open.
- In-flight requests are aborted when the phrase changes, so a slow response can
  never overwrite a newer one.
- Loading, empty and error states are handled explicitly; failures offer a
  retry action.
