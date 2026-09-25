import pymysql
import pytest

from fixtures.seeds import BASIC_SEED
from utils import config, db


@pytest.fixture(scope="session")
def db_connection():
    try:
        connection = db.connect()
    except pymysql.Error as error:
        raise RuntimeError(
            f"Could not connect to MySQL at {config.DB_HOST}:{config.DB_PORT}/{config.DB_NAME} "
            f"as '{config.DB_USER}': {error}. Start the database with 'docker compose up -d db'."
        ) from error

    try:
        yield connection
    finally:
        connection.close()


@pytest.fixture(scope="session")
def movies_repository(db_connection):
    return db.MoviesRepository(db_connection)


@pytest.fixture
def seeded_database(movies_repository):
    movies_repository.clear()
    movies_repository.seed(BASIC_SEED)

    try:
        yield movies_repository
    finally:
        movies_repository.clear()
