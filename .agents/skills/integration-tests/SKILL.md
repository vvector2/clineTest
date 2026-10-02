---
name: integration-tests
description: >-
  This skill is used to write and run integration test for current feature. It
  is used to verify that the feature works as expected when integrated with
  other components of the system.
---

## Steps

1. Verify whether all services from docker compose are running if not run `docker compose up`
2. Verify if Api is running
3. Run test by executing in folder test command `pytest -v --maxfail=1 `
4. If test fails please fix the existing test and run the test again
5. If test passes please add new integration test for new feature and run the test again
