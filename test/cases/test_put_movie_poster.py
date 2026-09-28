import struct
import uuid
import zlib

from fixtures.seeds import SEED_MOVIE_ONE_ID, seed_movie

MAX_POSTER_SIZE_IN_BYTES = 10 * 1024 * 1024


def make_png_bytes(width=1, height=1):
    def chunk(kind, payload):
        body = kind + payload
        return struct.pack(">I", len(payload)) + body + struct.pack(">I", zlib.crc32(body) & 0xFFFFFFFF)

    pixels = b"".join(b"\x00" + b"\xff\x00\x00" * width for _ in range(height))
    header = struct.pack(">IIBBBBB", width, height, 8, 2, 0, 0, 0)

    return b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", header) + chunk(b"IDAT", zlib.compress(pixels)) + chunk(b"IEND", b"")


def test_put_movie_poster_uploads_poster_and_updates_movie(api_client, seeded_database):
    poster = make_png_bytes()

    response = api_client.put_movie_poster(SEED_MOVIE_ONE_ID, poster)

    assert response.status_code == 200

    poster_url = response.json()["posterUrl"]
    assert SEED_MOVIE_ONE_ID in poster_url
    assert poster_url.endswith(".png")

    stored = seeded_database.get(SEED_MOVIE_ONE_ID)
    assert stored["PosterUrl"] == poster_url

    fetched = api_client.get(f"/movies/{SEED_MOVIE_ONE_ID}")
    assert fetched.status_code == 200
    assert fetched.json()["posterUrl"] == poster_url

    downloaded = api_client.get(poster_url)
    assert downloaded.status_code == 200
    assert downloaded.content == poster


def test_put_movie_poster_replaces_the_previous_poster(api_client, seeded_database):
    first = make_png_bytes() + b"\r\n"
    second = make_png_bytes(width=2) + b"\r\n"

    first_url = api_client.put_movie_poster(SEED_MOVIE_ONE_ID, first, file_name="poster.png").json()["posterUrl"]
    second_url = api_client.put_movie_poster(SEED_MOVIE_ONE_ID, second, file_name="poster.jpg", content_type="image/jpeg").json()["posterUrl"]

    assert first_url != second_url
    assert seeded_database.get(SEED_MOVIE_ONE_ID)["PosterUrl"] == second_url
    assert api_client.get(second_url).content == second


def test_put_movie_poster_rejects_non_image_file(api_client, seeded_database):
    response = api_client.put_movie_poster(
        SEED_MOVIE_ONE_ID,
        b"this is not an image",
        file_name="notes.txt",
        content_type="text/plain",
    )

    assert response.status_code == 400
    assert seeded_database.get(SEED_MOVIE_ONE_ID)["PosterUrl"] == seed_movie(SEED_MOVIE_ONE_ID)["PosterUrl"]


def test_put_movie_poster_rejects_empty_file(api_client, seeded_database):
    response = api_client.put_movie_poster(SEED_MOVIE_ONE_ID, b"")

    assert response.status_code == 400
    assert seeded_database.get(SEED_MOVIE_ONE_ID)["PosterUrl"] == seed_movie(SEED_MOVIE_ONE_ID)["PosterUrl"]


def test_put_movie_poster_rejects_poster_larger_than_the_limit(api_client, seeded_database):
    oversized = make_png_bytes() + b"\x00" * MAX_POSTER_SIZE_IN_BYTES

    response = api_client.put_movie_poster(SEED_MOVIE_ONE_ID, oversized)

    assert response.status_code == 400
    assert seeded_database.get(SEED_MOVIE_ONE_ID)["PosterUrl"] == seed_movie(SEED_MOVIE_ONE_ID)["PosterUrl"]


def test_put_movie_poster_returns_not_found_for_unknown_movie(api_client, seeded_database):
    response = api_client.put_movie_poster(uuid.uuid4(), make_png_bytes())

    assert response.status_code == 404
