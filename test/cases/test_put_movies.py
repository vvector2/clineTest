import uuid

import pytest

from fixtures.seeds import BASIC_SEED, SEED_MOVIE_ONE_ID, SEED_MOVIE_TWO_ID, seed_movie


def update_movie_payload(**overrides):
    payload = {
        "title": "The Matrix Reloaded",
        "year": 2003,
        "description": "Neo and his allies race against time to stop the machine army.",
        "director": "Lana Wachowski",
        "overview": "The human city of Zion defends itself against the massive invasion of the machines.",
        "originalLanguage": "English",
        "posterUrl": "https://example.com/posters/the-matrix-reloaded.jpg",
    }
    payload.update(overrides)
    return payload


def test_put_movie_updates_the_movie_and_can_be_fetched(api_client, seeded_database):
    payload = update_movie_payload()

    response = api_client.put_movie(SEED_MOVIE_ONE_ID, payload)

    assert response.status_code == 200
    assert response.json() == {"id": SEED_MOVIE_ONE_ID, **payload}

    stored = seeded_database.get(SEED_MOVIE_ONE_ID)
    assert stored["Title"] == payload["title"]
    assert stored["Year"] == payload["year"]
    assert stored["Description"] == payload["description"]
    assert stored["Director"] == payload["director"]
    assert stored["Overview"] == payload["overview"]
    assert stored["OriginalLanguage"] == payload["originalLanguage"]
    assert stored["PosterUrl"] == payload["posterUrl"]

    fetched = api_client.get(f"/movies/{SEED_MOVIE_ONE_ID}")
    assert fetched.status_code == 200
    assert fetched.json() == {"id": SEED_MOVIE_ONE_ID, **payload}


def test_put_movie_does_not_touch_the_other_movies(api_client, seeded_database):
    response = api_client.put_movie(SEED_MOVIE_ONE_ID, update_movie_payload())

    assert response.status_code == 200
    assert seeded_database.count() == len(BASIC_SEED)
    assert api_client.get(f"/movies/{SEED_MOVIE_TWO_ID}").json()["title"] == seed_movie(SEED_MOVIE_TWO_ID)["Title"]


def test_put_movie_allows_clearing_the_year(api_client, seeded_database):
    response = api_client.put_movie(SEED_MOVIE_ONE_ID, update_movie_payload(year=0))

    assert response.status_code == 200
    assert response.json()["year"] == 0
    assert seeded_database.get(SEED_MOVIE_ONE_ID)["Year"] == 0


def test_put_movie_ignores_the_id_in_the_body(api_client, seeded_database):
    response = api_client.put_movie(SEED_MOVIE_ONE_ID, update_movie_payload(id=str(SEED_MOVIE_TWO_ID)))

    assert response.status_code == 200
    assert response.json()["id"] == SEED_MOVIE_ONE_ID
    assert seeded_database.get(SEED_MOVIE_ONE_ID)["Title"] == update_movie_payload()["title"]
    assert seeded_database.get(SEED_MOVIE_TWO_ID)["Title"] == seed_movie(SEED_MOVIE_TWO_ID)["Title"]


def test_put_movie_returns_not_found_for_unknown_movie(api_client, seeded_database):
    response = api_client.put_movie(uuid.uuid4(), update_movie_payload())

    assert response.status_code == 404
    assert seeded_database.count() == len(BASIC_SEED)


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
def test_put_movie_rejects_invalid_payload(api_client, seeded_database, overrides):
    response = api_client.put_movie(SEED_MOVIE_ONE_ID, update_movie_payload(**overrides))

    assert response.status_code == 400
    assert seeded_database.get(SEED_MOVIE_ONE_ID)["Title"] == seed_movie(SEED_MOVIE_ONE_ID)["Title"]