import urllib3

from fixtures.api import api_client, api_is_available
from fixtures.database import db_connection, movies_repository, seeded_database
from utils import config

if not config.API_VERIFY_TLS:
    urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

