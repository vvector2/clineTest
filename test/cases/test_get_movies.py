import uuid

from fixtures.seeds import BASIC_SEED, SEED_MOVIE_ONE_ID, SEED_MOVIE_TWO_ID, seed_movie, to_api_json


def test_get_movies_returns_all_seeded_movies(api_client, seeded_database):
    response = api_client.get("/movies")

    assert response.status_code == 200

    expected = sorted((to_api_json(movie) for movie in BASIC_SEED), key=lambda movie: movie["id"])
    actual = sorted(response.json(), key=lambda movie: movie["id"])

    assert actual == expected


def test_get_movies_returns_empty_list_when_database_is_empty(api_client, seeded_database):
    seeded_database.clear()

    response = api_client.get("/movies")

    assert response.status_code == 200
    assert response.json() == []


def test_get_movie_by_id_returns_seeded_movie(api_client, seeded_database):
    response = api_client.get(f"/movies/{SEED_MOVIE_ONE_ID}")

    assert response.status_code == 200
    assert response.json() == to_api_json(seed_movie(SEED_MOVIE_ONE_ID))


def test_get_movie_by_id_returns_movie_from_a_clean_seed(api_client, seeded_database):
    assert seeded_database.count() == len(BASIC_SEED)

    response = api_client.get(f"/movies/{SEED_MOVIE_TWO_ID}")

    assert response.status_code == 200
    assert response.json()["id"] == SEED_MOVIE_TWO_ID


def test_get_movie_by_id_returns_not_found_for_unknown_id(api_client, seeded_database):
    response = api_client.get(f"/movies/{uuid.uuid4()}")

    assert response.status_code == 404
