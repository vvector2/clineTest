"""Seeds the movie database of ClientTestApi through its HTTP endpoints.

Every movie metadata file and poster image found in the folder given as argument is
read and turned into a movie. The movie is created with ``POST /api/movies`` and, when a
poster image belongs to it, the image is uploaded afterwards with
``PUT /api/movies/poster`` so the movie points at the image in the S3 bucket instead of
the placeholder URL.

The folder is searched recursively, so both layouts work:

    movies_2025_json/27205.json  and  posters_2025/27205.jpg
    27205.json                   and  27205.jpg

A poster is matched to a movie by its file name, which has to be the id of the movie
file, exactly the way ``scripts/pull_movies.py`` writes them. Files that are not TMDb
movie payloads, such as ``job_state.json``, are ignored.

The metadata files are the TMDb discover payloads written by ``scripts/pull_movies.py``.
Fields the payload does not contain, such as the director, fall back to a placeholder so
the request satisfies the validation of the API.

Start the infrastructure and the API before running the script:

    docker compose up -d
    dotnet run --project Backend/ClientTestApi

Usage:

    python scripts/seed-database.py scripts
"""

import argparse
import json
import os
import sys
from pathlib import Path

import requests

TMDB_IMAGE_BASE_URL = "https://image.tmdb.org/t/p/w500"

PLACEHOLDER_POSTER_URL = "https://placehold.co/500x750/png?text=No+Poster"
PLACEHOLDER_DIRECTOR = "Unknown"
PLACEHOLDER_TEXT = "Unknown"

MAX_TITLE_LENGTH = 200
MAX_DESCRIPTION_LENGTH = 1000
MAX_DIRECTOR_LENGTH = 200
MAX_OVERVIEW_LENGTH = 2000
MAX_ORIGINAL_LANGUAGE_LENGTH = 100
MAX_POSTER_URL_LENGTH = 500

IMAGE_CONTENT_TYPES = {
    ".jpg": "image/jpeg",
    ".jpeg": "image/jpeg",
    ".png": "image/png",
    ".gif": "image/gif",
    ".webp": "image/webp",
}

# TMDb reports ISO 639-1 codes, the API stores and displays the language name.
LANGUAGE_NAMES = {
    "ar": "Arabic",
    "cs": "Czech",
    "da": "Danish",
    "de": "German",
    "el": "Greek",
    "en": "English",
    "es": "Spanish",
    "fa": "Persian",
    "fi": "Finnish",
    "fr": "French",
    "he": "Hebrew",
    "hi": "Hindi",
    "hu": "Hungarian",
    "id": "Indonesian",
    "it": "Italian",
    "ja": "Japanese",
    "ko": "Korean",
    "ms": "Malay",
    "nl": "Dutch",
    "no": "Norwegian",
    "pl": "Polish",
    "pt": "Portuguese",
    "ro": "Romanian",
    "ru": "Russian",
    "sv": "Swedish",
    "ta": "Tamil",
    "te": "Telugu",
    "th": "Thai",
    "tr": "Turkish",
    "uk": "Ukrainian",
    "vi": "Vietnamese",
    "zh": "Chinese",
}


def parse_args():
    parser = argparse.ArgumentParser(
        description="Seed the movie database of ClientTestApi with the movies and posters found in a folder.",
        epilog="Example: python scripts/seed-database.py scripts",
    )
    parser.add_argument(
        "folder",
        help="Folder that holds the movie metadata files and the poster images.",
    )
    parser.add_argument(
        "--api-base-url",
        dest="api_base_url",
        default=os.getenv("CLINETEST_API_BASE_URL", "https://localhost:7072"),
        help="Base address of ClientTestApi without the /api suffix.",
    )
    parser.add_argument(
        "--timeout",
        type=float,
        default=float(os.getenv("CLINETEST_API_TIMEOUT_SECONDS", "30")),
        help="Timeout of a single API call in seconds.",
    )
    parser.add_argument(
        "--verify-tls",
        dest="verify_tls",
        action="store_true",
        default=os.getenv("CLINETEST_API_VERIFY_TLS", "false").strip().lower() in ("1", "true", "yes", "on"),
        help="Verify the TLS certificate of the API. The development certificate is not trusted by default.",
    )
    parser.add_argument(
        "--skip-posters",
        dest="skip_posters",
        action="store_true",
        help="Create the movies without uploading the poster images.",
    )
    parser.add_argument(
        "--dry-run",
        dest="dry_run",
        action="store_true",
        help="Print what would be sent without calling the API.",
    )
    parser.add_argument(
        "--limit",
        type=int,
        default=0,
        help="Seed at most this many movies. Every movie is seeded when it is not set.",
    )
    return parser.parse_args()


def truncate(text, max_length):
    """Shorten text to max_length, ending with an ellipsis when it was cut."""
    text = (text or "").strip()

    if len(text) <= max_length:
        return text

    return text[: max_length - 3].rstrip() + "..."


def get_text(movie, *keys):
    """Return the first non-empty string value of keys, or an empty string."""
    for key in keys:
        value = movie.get(key)

        if isinstance(value, str) and value.strip():
            return value.strip()

    return ""


def get_year(movie):
    """Return the release year of a TMDb movie, or 0 when it has no release date."""
    release_date = get_text(movie, "release_date", "releaseDate")

    if len(release_date) >= 4 and release_date[:4].isdigit():
        return int(release_date[:4])

    return 0


def get_language(movie):
    """Return the original language name of a TMDb movie."""
    code = get_text(movie, "original_language", "originalLanguage")

    if not code:
        return PLACEHOLDER_TEXT

    return LANGUAGE_NAMES.get(code.lower(), code.upper())


def get_poster_url(movie):
    """Return the TMDb image URL of the movie poster, or a placeholder when it has none."""
    poster_path = get_text(movie, "poster_path", "posterPath")

    if not poster_path:
        return PLACEHOLDER_POSTER_URL

    if not poster_path.startswith("/"):
        poster_path = "/" + poster_path

    return TMDB_IMAGE_BASE_URL + poster_path


def build_payload(movie):
    """Turn a TMDb movie payload into the body of POST /api/movies."""
    overview = get_text(movie, "overview")

    return {
        "title": truncate(get_text(movie, "title", "original_title", "name") or PLACEHOLDER_TEXT, MAX_TITLE_LENGTH),
        "year": get_year(movie),
        "description": truncate(overview or PLACEHOLDER_TEXT, MAX_DESCRIPTION_LENGTH),
        "director": truncate(get_text(movie, "director") or PLACEHOLDER_DIRECTOR, MAX_DIRECTOR_LENGTH),
        "overview": truncate(overview or PLACEHOLDER_TEXT, MAX_OVERVIEW_LENGTH),
        "originalLanguage": truncate(get_language(movie), MAX_ORIGINAL_LANGUAGE_LENGTH),
        "posterUrl": truncate(get_poster_url(movie), MAX_POSTER_URL_LENGTH),
    }


def load_movie(path):
    """Read a TMDb movie file, or return None when the file is not a movie payload."""
    try:
        with open(path, "r", encoding="utf-8") as handle:
            movie = json.load(handle)
    except (OSError, json.JSONDecodeError) as error:
        print(f"Could not read movie file '{path}': {error}")
        return None

    if not isinstance(movie, dict) or not get_text(movie, "title", "original_title", "name"):
        return None

    return movie


def index_posters(folder):
    """Return every poster image in the folder keyed by its file name without extension."""
    posters = {}

    for path in folder.rglob("*"):
        if not path.is_file():
            continue

        if path.suffix.lower() not in IMAGE_CONTENT_TYPES:
            continue

        posters.setdefault(path.stem, path)

    return posters


def find_movies(folder):
    """Return the TMDb movie files of the folder, sorted by file name."""
    movies = []

    for path in sorted(folder.rglob("*.json")):
        if not path.is_file():
            continue

        movie = load_movie(path)

        if movie is None:
            continue

        movies.append((path, movie))

    return movies


def create_movie(session, url, payload, timeout):
    """Create a movie and return its id."""
    response = session.post(url, json=payload, timeout=timeout)
    response.raise_for_status()

    return response.json().strip('"')


def upload_poster(session, url, movie_id, poster_path, timeout):
    """Upload the poster of a movie so its poster URL points at the S3 bucket."""
    content_type = IMAGE_CONTENT_TYPES[poster_path.suffix.lower()]

    with open(poster_path, "rb") as handle:
        response = session.put(
            url,
            data={"MovieId": movie_id},
            files={"Poster": (poster_path.name, handle, content_type)},
            timeout=timeout,
        )

    response.raise_for_status()

    return response.json()["posterUrl"]


def seed(session, movies_url, posters_url, movies, posters, args):
    """Seed the given movies and return the number of failures."""
    created = 0
    posters_uploaded = 0
    failures = 0

    for index, (path, movie) in enumerate(movies, start=1):
        payload = build_payload(movie)
        poster_path = posters.get(path.stem)
        prefix = f"[{index}/{len(movies)}] {payload['title']}"

        if args.dry_run:
            print(f"{prefix} would be posted as {json.dumps(payload, ensure_ascii=False)}")
            continue

        try:
            movie_id = create_movie(session, movies_url, payload, args.timeout)
        except (requests.RequestException, ValueError) as error:
            failures += 1
            print(f"{prefix} could not be created: {error}")
            continue

        created += 1
        print(f"{prefix} created as {movie_id}")

        if args.skip_posters:
            continue

        if poster_path is None:
            print(f"{prefix} has no poster image, the placeholder URL is kept.")
            continue

        try:
            poster_url = upload_poster(session, posters_url, movie_id, poster_path, args.timeout)
        except (requests.RequestException, OSError, ValueError, KeyError) as error:
            failures += 1
            print(f"{prefix} was created but its poster could not be uploaded: {error}")
            continue

        posters_uploaded += 1
        print(f"{prefix} poster uploaded as {poster_url}")

    if not args.dry_run:
        print(f"Created {created} movies and uploaded {posters_uploaded} posters, {failures} failures.")

    return failures


def main():
    args = parse_args()
    folder = Path(args.folder).expanduser()

    if not folder.is_dir():
        print(f"The folder '{folder}' does not exist.")
        return 1

    movies = find_movies(folder)

    if args.limit > 0:
        movies = movies[: args.limit]

    posters = {} if args.skip_posters else index_posters(folder)
    movies_url = f"{args.api_base_url.rstrip('/')}/api/movies"
    posters_url = f"{movies_url}/poster"

    with requests.Session() as session:
        session.verify = args.verify_tls

        if not args.dry_run:
            try:
                response = session.get(movies_url, timeout=args.timeout)
                response.raise_for_status()
            except requests.RequestException as error:
                print(f"The API is not reachable at '{movies_url}': {error}")
                print("Start it with 'docker compose up -d' and 'dotnet run --project Backend/ClientTestApi'.")
                return 1

            print(f"API '{movies_url}' is reachable, seeding {len(movies)} movies.")
        else:
            print(f"Would seed {len(movies)} movies into '{movies_url}'.")

        failures = seed(session, movies_url, posters_url, movies, posters, args)

    return 1 if failures else 0


if __name__ == "__main__":
    sys.exit(main())