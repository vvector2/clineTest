import uuid

from fixtures.seeds import BASIC_SEED


def create_movie_payload(**overrides):
    payload = {
        "title": "The Godfather",
        "year": 1972,
        "description": "The aging patriarch of an organized crime dynasty transfers control to his reluctant son.",
        "director": "Francis Ford Coppola",
        "overview": "The story of the Corleone family under patriarch Vito Corleone.",
        "originalLanguage": "English",
        "posterUrl": "https://example.com/posters/the-godfather.jpg",
    }
    payload.update(overrides)
    return payload


def index_movie(api_client, **overrides):
    response = api_client.post("/movies", json=create_movie_payload(**overrides))

    assert response.status_code == 201

    return response.json()


def search(api_client, query):
    response = api_client.get("/movies", params={"query": query})

    assert response.status_code == 200

    return response.json()


def test_get_movies_with_query_returns_matching_movie(api_client, seeded_database):
    created_id = index_movie(api_client)

    results = search(api_client, "Godfather")

    assert created_id in [movie["id"] for movie in results]


def test_get_movies_with_query_matches_misspelled_phrase(api_client, seeded_database):
    created_id = index_movie(api_client)

    results = search(api_client, "godfathr")

    assert created_id in [movie["id"] for movie in results]


def test_get_movies_with_query_matches_director(api_client, seeded_database):
    created_id = index_movie(api_client)

    results = search(api_client, "Coppola")

    assert created_id in [movie["id"] for movie in results]


def test_get_movies_with_query_returns_the_matching_movie_fields(api_client, seeded_database):
    created_id = index_movie(api_client)

    results = search(api_client, "Godfather")
    movie = next(movie for movie in results if movie["id"] == created_id)

    assert movie["title"] == "The Godfather"
    assert movie["director"] == "Francis Ford Coppola"
    assert movie["year"] == 1972


def test_get_movies_with_query_returns_empty_list_when_nothing_matches(api_client, seeded_database):
    index_movie(api_client)

    results = search(api_client, uuid.uuid4().hex)

    assert results == []


def test_get_movies_with_empty_query_returns_all_movies(api_client, seeded_database):
    results = search(api_client, "")

    assert len(results) == len(BASIC_SEED)
