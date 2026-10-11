# clinetest backend on Kubernetes

Infrastructure-as-code for running the `ClientTestApi` backend and its database
on a local Kubernetes cluster, behind a load balancer.

## What gets deployed

| Workload | Kind | Image | Notes |
| --- | --- | --- | --- |
| MySQL | StatefulSet | `mysql:8.4` | 5Gi PVC, matches `docker-compose.yaml` |
| Elasticsearch | StatefulSet | `elasticsearch:8.15.3` | 5Gi PVC, required at startup |
| LocalStack (S3) | StatefulSet | `localstack/localstack:4.14.0` | S3 only, matches compose |
| clinetest-api | Deployment (2 replicas) | built from `Backend/` | behind the load balancer |
| clinetest-api-lb | Service `LoadBalancer` | - | external entry point |
| clinetest-db-migrate | Job (one-shot) | built from `Backend/` | applies EF Core migrations |

The backend cannot start without S3 and Elasticsearch, because
`ClineTestInfra\InfraServiceCollectionExtensions.cs` registers all three
adapters unconditionally and each one throws when its configuration is missing.
Elasticsearch is therefore part of this stack even though the request was for
"backend and database".

## Usage

Run from WSL (the shell has `docker`, `kubectl` and `minikube`):

```bash
./iac/backend/scripts/up.sh      # build images, start cluster, deploy, migrate
./iac/backend/scripts/status.sh  # pods, services, external IP, PVCs
./iac/backend/scripts/down.sh    # delete the cluster
```

`down.sh --keep-cluster` removes just the workloads and keeps the cluster.

The scripts derive their repository path from their own location, so they work
from any working directory.

## Reaching the API

On minikube the `LoadBalancer` Service is reachable through the NodePort that
Kubernetes assigns to it:

```bash
minikube service list -p clinetest      # shows clinetest-api-lb  http/80
minikube service clinetest-api-lb -n clinetest -p clinetest   # opens a tunnel
```

From inside the cluster, or from WSL once you know the NodePort:

```bash
curl http://$(minikube ip -p clinetest):<nodePort>/api/movies
```

To point the integration test suite at the cluster:

```bash
export CLINETEST_API_BASE_URL="http://<EXTERNAL_IP>"
export CLINETEST_API_VERIFY_TLS=false
pytest test
```

Swagger is not reachable: the API only enables Swagger in the Development
environment and the Deployment sets `ASPNETCORE_ENVIRONMENT=Production`.

On minikube the `EXTERNAL-IP` column stays `<pending>` because the bundled
MetalLB addon ships without its CRDs, so there is no address pool to allocate
from. The Service still works through its NodePort. On kind, install the
MetalLB addon and configure an `IPAddressPool` to get a real external IP; on a
managed cluster the same Service provisions a cloud load balancer instead.

## Design notes

**Migrations run from a prebuilt bundle.** `Dockerfile.migrator` calls
`dotnet ef migrations bundle` at image build time, and the resulting image
contains only the .NET runtime and the bundle. This follows the EF Core guidance
for automated deployment: a bundle is a self-contained artifact that runs
without the .NET SDK, the EF tools, or the application source
(<https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying>).
Shipping the SDK and source into a Job image would work but contradicts that
guidance.

**The API waits for MySQL.** `AddDal()` calls `ServerVersion.AutoDetect()`, which
opens a real connection while the host is being built and throws if MySQL is not
up yet. An init container blocks on the MySQL port so the Deployment does not
enter a crash-backoff loop during startup.

**Probes are TCP-based.** The API exposes no health endpoint, so the Deployment
uses `tcpSocket` probes rather than adding application code just for
Kubernetes. Adding a real `/health` endpoint would be the better long-term fix.

**Storage is stateful and single-replica.** Each stateful workload has one
replica with a `ReadWriteOnce` PVC. This is a faithful-but-simple deployment:
running MySQL or Elasticsearch on a single PVC is not highly available, and
`down.sh` deletes the data. Use managed database services for anything real.

**Secrets are committed for local use only.** `05-secret.yaml` contains the same
development credentials as `docker-compose.yaml`. Replace it with an external
secret store before using this anywhere shared.

## Portability

The manifests use `type: LoadBalancer` and no cloud-specific controllers, so the
same files apply to kind (with the MetalLB addon), k3d, EKS or AKS. Only
`clinetest-api-lb` changes meaning: on minikube it gets an IP from the bundled
MetalLB, on a managed cluster it provisions a cloud load balancer.