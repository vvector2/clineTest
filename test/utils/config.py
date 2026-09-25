import os

API_BASE_URL = os.getenv("CLINETEST_API_BASE_URL", "https://localhost:7072").rstrip("/")
API_VERIFY_TLS = os.getenv("CLINETEST_API_VERIFY_TLS", "false").strip().lower() in ("1", "true", "yes", "on")
API_TIMEOUT_SECONDS = float(os.getenv("CLINETEST_API_TIMEOUT_SECONDS", "30"))

DB_HOST = os.getenv("CLINETEST_DB_HOST", "localhost")
DB_PORT = int(os.getenv("CLINETEST_DB_PORT", "3306"))
DB_NAME = os.getenv("CLINETEST_DB_NAME", "clinetest")
DB_USER = os.getenv("CLINETEST_DB_USER", "clinetest")
DB_PASSWORD = os.getenv("CLINETEST_DB_PASSWORD", "clinetest")
DB_CHARSET = os.getenv("CLINETEST_DB_CHARSET", "utf8mb4")
