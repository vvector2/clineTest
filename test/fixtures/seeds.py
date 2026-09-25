SEED_MOVIE_ONE_ID = "11111111-1111-4111-8111-111111111111"
SEED_MOVIE_TWO_ID = "22222222-2222-4222-8222-222222222222"
SEED_MOVIE_THREE_ID = "33333333-3333-4333-8333-333333333333"

BASIC_SEED = (
    {
        "Id": SEED_MOVIE_ONE_ID,
        "Title": "Seed Movie One",
        "Year": 2001,
        "Description": "First movie inserted by the basic seed.",
        "Director": "Seed Director One",
        "Overview": "Overview of the first movie inserted by the basic seed.",
        "OriginalLanguage": "English",
        "PosterUrl": "https://example.com/posters/seed-one.jpg",
    },
    {
        "Id": SEED_MOVIE_TWO_ID,
        "Title": "Seed Movie Two",
        "Year": 2002,
        "Description": "Second movie inserted by the basic seed.",
        "Director": "Seed Director Two",
        "Overview": "Overview of the second movie inserted by the basic seed.",
        "OriginalLanguage": "Spanish",
        "PosterUrl": "https://example.com/posters/seed-two.jpg",
    },
    {
        "Id": SEED_MOVIE_THREE_ID,
        "Title": "Seed Movie Three",
        "Year": 2003,
        "Description": "Third movie inserted by the basic seed.",
        "Director": "Seed Director Three",
        "Overview": "Overview of the third movie inserted by the basic seed.",
        "OriginalLanguage": "Korean",
        "PosterUrl": "https://example.com/posters/seed-three.jpg",
    },
)

MOVIES_BY_ID = {movie["Id"]: movie for movie in BASIC_SEED}


def seed_movie(movie_id):
    return MOVIES_BY_ID[movie_id]


def to_api_json(movie):
    return {
        "id": movie["Id"],
        "title": movie["Title"],
        "year": movie["Year"],
        "description": movie["Description"],
        "director": movie["Director"],
        "overview": movie["Overview"],
        "originalLanguage": movie["OriginalLanguage"],
        "posterUrl": movie["PosterUrl"],
    }
