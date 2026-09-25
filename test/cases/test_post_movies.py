import uuid

import pytest

from fixtures.seeds import BASIC_SEED


def create_movie_payload(**overrides):
    payload = {
        "title": "The Matrix",
        "year": 1999,
        "description": "A computer hacker learns the true nature of his reality.",
        "director": "Lana Wachowski",
        "overview": "Neo is contacted by a mysterious stranger and has to choose between two realities.",
        "originalLanguage": "English",
        "posterUrl": "https://example.com/posters/the-matrix.jpg",
    }
    payload.update(overrides)
    return payload


def test_post_movies_creates_movie_and_can_be_fetched(api_client, seeded_database):
    payload = create_movie_payload()

    response = api_client.post("/movies", json=payload)

    assert response.status_code == 201
    created_id = response.json()
    assert uuid.UUID(created_id)

    stored = seeded_database.get(created_id)
    assert stored is not None
    assert stored["Title"] == payload["title"]
    assert stored["Year"] == payload["year"]
    assert stored["PosterUrl"] == payload["posterUrl"]
    assert seeded_database.count() == len(BASIC_SEED) + 1

    fetched = api_client.get(f"/movies/{created_id}")
    assert fetched.status_code == 200
    assert fetched.json()["title"] == payload["title"]


@pytest.mark.parametrize(
    "overrides",
    [
        {"title": None},
        {"title": ""},
        {"title": "x" * 201},
        {"director": None},
        {"originalLanguage": None},
        {"posterUrl": "not-a-url"},
        {"year": "not-a-number"},
    ],
)
def test_post_movies_rejects_invalid_payload(api_client, seeded_database, overrides):
    response = api_client.post("/movies", json=create_movie_payload(**overrides))

    assert response.status_code == 400
    assert seeded_database.count() == len(BASIC_SEED)
