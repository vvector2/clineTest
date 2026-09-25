import pytest
import requests

from utils.api_client import ApiClient


@pytest.fixture(scope="session")
def api_client():
    client = ApiClient()

    try:
        yield client
    finally:
        client.close()


@pytest.fixture(scope="session", autouse=True)
def api_is_available(api_client):
    try:
        api_client.get("/movies")
    except requests.RequestException as error:
        raise RuntimeError(
            f"The API is not reachable at {api_client.base_url}: {error}. "
            "Start it with 'dotnet run --project Backend/ClientTestApi'."
        ) from error
