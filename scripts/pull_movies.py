import argparse
import json
import os
from datetime import datetime, timezone

import requests

# TMDb Base URLs
DISCOVER_URL = "https://api.themoviedb.org/3/discover/movie"
IMAGE_BASE_URL = "https://image.tmdb.org/t/p/w500"  # w500 gives 500px width poster images

# Last job record, used to pause and resume a pull
JOB_STATE_FILE = "job_state.json"

def utc_now():
    return datetime.now(timezone.utc).isoformat(timespec="seconds")

def load_job_state(state_file):
    if not os.path.exists(state_file):
        return None

    try:
        with open(state_file, "r", encoding="utf-8") as f:
            return json.load(f)
    except (OSError, json.JSONDecodeError) as error:
        print(f"Could not read job record '{state_file}': {error}")
        return None

def save_job_state(state_file, job):
    job["updated_at"] = utc_now()
    with open(state_file, "w", encoding="utf-8") as f:
        json.dump(job, f, indent=4, ensure_ascii=False)

def build_job_record(year, json_dir, poster_dir):
    return {
        "script": os.path.basename(__file__),
        "year": year,
        "status": "running",
        "started_at": utc_now(),
        "updated_at": utc_now(),
        "json_dir": json_dir,
        "poster_dir": poster_dir,
        "last_processed_page": 0,
        "next_page": 1,
        "total_pages": None,
        "total_results": None,
        "movies_saved": 0,
        "posters_downloaded": 0,
        "last_movie_id": None,
        "last_movie_title": None,
    }

def restore_job_progress(job, record):
    for key in (
        "started_at",
        "total_pages",
        "total_results",
        "movies_saved",
        "posters_downloaded",
        "last_movie_id",
        "last_movie_title",
    ):
        if record.get(key) is not None:
            job[key] = record[key]

    job["last_processed_page"] = record.get("last_processed_page") or 0
    job["next_page"] = job["last_processed_page"] + 1

    return job

def fetch_and_save_movies(api_key, year, resume=False, max_pages=None):
    # Setup output folders
    json_dir = f"movies_{year}_json"
    poster_dir = f"posters_{year}"

    os.makedirs(json_dir, exist_ok=True)
    os.makedirs(poster_dir, exist_ok=True)

    job = build_job_record(year, json_dir, poster_dir)

    if resume:
        record = load_job_state(JOB_STATE_FILE)

        if record is None:
            print(f"Nothing to resume: no job record found in '{JOB_STATE_FILE}'.")
            return

        if record.get("year") != year:
            print(f"Job record in '{JOB_STATE_FILE}' is for year {record.get('year')}, not {year}. Nothing resumed.")
            return

        if record.get("status") == "completed":
            print(f"Job for year {year} is already completed according to '{JOB_STATE_FILE}'.")
            return

        job = restore_job_progress(job, record)
        print(f"Resuming job for year {year} from page {job['next_page']}.")

    page = job["next_page"]
    total_pages = job["total_pages"] or 1
    pages_this_run = 0

    save_job_state(JOB_STATE_FILE, job)

    headers = {
        "accept": "application/json",
    }

    print(f"Starting fetch for year {year}...")

    try:
        while page <= total_pages:
            if max_pages is not None and pages_this_run >= max_pages:
                job["status"] = "paused"
                print(f"Paused after {pages_this_run} page(s) in this run (--max-pages {max_pages}).")
                break

            params = {
                "api_key": api_key,
                "primary_release_year": year,
                "sort_by": "popularity.desc",
                "page": page,
                "language": "en-US"
            }

            response = requests.get(DISCOVER_URL, headers=headers, params=params)

            if response.status_code != 200:
                job["status"] = "failed"
                print(f"Failed to fetch page {page}: {response.status_code}")
                break

            data = response.json()

            if job["total_pages"] is None:
                total_pages = data.get("total_pages", 1)
                job["total_pages"] = total_pages
                job["total_results"] = data.get("total_results", 0)
                print(f"Total movies found: {job['total_results']} across {total_pages} pages.")

            movies = data.get("results", [])

            for movie in movies:
                movie_id = movie["id"]

                # 1. Save Movie JSON metadata
                json_filename = os.path.join(json_dir, f"{movie_id}.json")
                with open(json_filename, "w", encoding="utf-8") as f:
                    json.dump(movie, f, indent=4, ensure_ascii=False)
                job["movies_saved"] += 1

                # 2. Download Poster Image (if available)
                poster_path = movie.get("poster_path")
                if poster_path:
                    poster_url = f"{IMAGE_BASE_URL}{poster_path}"
                    poster_filename = os.path.join(poster_dir, f"{movie_id}.jpg")

                    # Avoid re-downloading existing images
                    if not os.path.exists(poster_filename):
                        img_data = requests.get(poster_url).content
                        with open(poster_filename, "wb") as f:
                            f.write(img_data)
                        job["posters_downloaded"] += 1

                job["last_movie_id"] = movie_id
                job["last_movie_title"] = movie.get("title")

            job["last_processed_page"] = page
            job["next_page"] = page + 1
            pages_this_run += 1
            save_job_state(JOB_STATE_FILE, job)

            print(f"Processed page {page}/{total_pages}")
            page += 1
    except KeyboardInterrupt:
        job["status"] = "paused"
        print(f"\nInterrupted; progress saved to '{JOB_STATE_FILE}'.")
    except requests.RequestException as error:
        job["status"] = "failed"
        print(f"Request error while processing page {page}: {error}")

    if job["status"] == "running":
        job["status"] = "completed"

    save_job_state(JOB_STATE_FILE, job)

    print(
        f"Job {job['status']}: page {job['last_processed_page']}/{job['total_pages']} processed, "
        f"movies saved: {job['movies_saved']}, posters downloaded: {job['posters_downloaded']}."
    )

    if job["status"] != "completed":
        print(f"Resume with: python {os.path.basename(__file__)} --api-key <API_KEY> --year {year} --resume")

def parse_args():
    parser = argparse.ArgumentParser(
        description="Fetch movies from TMDb for a given year and save metadata plus poster images.",
        epilog=f"Progress is tracked in '{JOB_STATE_FILE}' so a job can be paused and resumed.",
    )
    parser.add_argument(
        "--api-key",
        required=True,
        help="TMDb API key used to authenticate against the TMDb API.",
    )
    parser.add_argument(
        "--year",
        required=True,
        type=int,
        help="Primary release year to fetch movies for (e.g. 2025).",
    )
    parser.add_argument(
        "--resume",
        action="store_true",
        help=f"Resume the job recorded in '{JOB_STATE_FILE}' for the given year.",
    )
    parser.add_argument(
        "--max-pages",
        dest="max_pages",
        type=int,
        default=None,
        help="Pause the job after processing this many pages in the current run.",
    )
    return parser.parse_args()


if __name__ == "__main__":
    args = parse_args()
    fetch_and_save_movies(args.api_key, args.year, resume=args.resume, max_pages=args.max_pages)