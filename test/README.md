# Movies API integration tests

Pytest based integration tests that exercise the running `ClientTestApi` over HTTP against the
real MySQL database, the LocalStack S3 bucket and Elasticsearch.

## Layout

```
test/
  cases/       pytest test modules (test_*.py)
  fixtures/    pytest fixtures and the seed data the cases rely on
  utils/       supporting modules (configuration, HTTP client, database access)
  conftest.py  registers the fixtures for the whole suite
  pytest.ini   test settings and import paths
```

`utils` and `fixtures` are importable because `pytest.ini` lists the suite root on `pythonpath`.

## Prerequisites

1. Start the infrastructure:

   ```bash
   docker compose up -d
   ```

2. Start the API (the `https` profile exposes `https://localhost:7072`):

   ```bash
   dotnet run --project Backend/ClientTestApi
   ```

## Environment

The environment is managed with `pipenv` and pinned to Python 3.12:

```bash
cd test
python -m pipenv install --dev
```

`pipenv` loads `test/.env` automatically. Copy `.env.example` to `.env` to override the defaults.

`pipenv` is called as `python -m pipenv` because the interpreter lives in `C:\Python312`, whose `Scripts`
folder is only writable by `SYSTEM` and `Administrators`, so the `pipenv.exe` shim cannot be installed
there. `python -m pipenv` behaves exactly like the CLI.

## Running

```bash
cd test
python -m pipenv run pytest
```
