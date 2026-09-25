import pymysql
import pymysql.cursors

from utils import config

TABLE = "movies"
COLUMNS = ("Id", "Title", "Year", "Description", "Director", "Overview", "OriginalLanguage", "PosterUrl")


def connect():
    return pymysql.connect(
        host=config.DB_HOST,
        port=config.DB_PORT,
        user=config.DB_USER,
        password=config.DB_PASSWORD,
        database=config.DB_NAME,
        charset=config.DB_CHARSET,
        cursorclass=pymysql.cursors.DictCursor,
        autocommit=True,
    )


class MoviesRepository:
    def __init__(self, connection):
        self._connection = connection

    def clear(self):
        with self._connection.cursor() as cursor:
            cursor.execute(f"DELETE FROM {TABLE}")

    def seed(self, movies):
        if not movies:
            return

        placeholders = ", ".join(["%s"] * len(COLUMNS))
        statement = f"INSERT INTO {TABLE} ({', '.join(COLUMNS)}) VALUES ({placeholders})"
        rows = [tuple(movie[column] for column in COLUMNS) for movie in movies]

        with self._connection.cursor() as cursor:
            cursor.executemany(statement, rows)

    def all(self):
        with self._connection.cursor() as cursor:
            cursor.execute(f"SELECT {', '.join(COLUMNS)} FROM {TABLE}")
            return cursor.fetchall()

    def get(self, movie_id):
        with self._connection.cursor() as cursor:
            cursor.execute(f"SELECT {', '.join(COLUMNS)} FROM {TABLE} WHERE Id = %s", (str(movie_id),))
            return cursor.fetchone()

    def count(self):
        with self._connection.cursor() as cursor:
            cursor.execute(f"SELECT COUNT(*) AS Total FROM {TABLE}")
            return cursor.fetchone()["Total"]
