import requests

from utils import config


class ApiClient:
    def __init__(self, base_url=None, verify=None, timeout=None):
        self._base_url = (base_url or config.API_BASE_URL).rstrip("/")
        self._timeout = timeout if timeout is not None else config.API_TIMEOUT_SECONDS
        self._session = requests.Session()
        self._session.verify = config.API_VERIFY_TLS if verify is None else verify

    @property
    def base_url(self):
        return self._base_url

    def get(self, path, **kwargs):
        return self._request("GET", path, **kwargs)

    def post(self, path, **kwargs):
        return self._request("POST", path, **kwargs)

    def put(self, path, **kwargs):
        return self._request("PUT", path, **kwargs)

    def put_movie_poster(self, movie_id, content, file_name="poster.png", content_type="image/png"):
        return self.put(
            "/movies/poster",
            data={"MovieId": str(movie_id)},
            files={"Poster": (file_name, content, content_type)},
        )

    def close(self):
        self._session.close()

    def _request(self, method, path, **kwargs):
        return self._session.request(method, self._url(path), timeout=self._timeout, **kwargs)

    def _url(self, path):
        if path.startswith(("http://", "https://")):
            return path

        return f"{self._base_url}{config.API_PREFIX}/{path.lstrip('/')}"
