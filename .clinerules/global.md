# General rules

Docker, kubectl, minikube is not existing on this env. It is on WSL so propably you need to run something like
`wsl.exe -- bash -lc "docker compose down"`

All env variables need to run ClientTestApi locally for debug or integration test need to specified and up to date in Backend\ClientTestApi\Properties\launchSettings.json
