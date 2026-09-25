from fixtures.api import api_client, api_is_available
from fixtures.database import db_connection, movies_repository, seeded_database
from utils.config import API_VERIFY_TLS

INSECURE_REQUEST_WARNING_FILTER = "ignore::urllib3.exceptions.InsecureRequestWarning"


def pytest_configure(config):
    if not API_VERIFY_TLS:
        config.addinivalue_line("filterwarnings", INSECURE_REQUEST_WARNING_FILTER)

