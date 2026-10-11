#!/usr/bin/env bash
# Builds the API and migration-bundle images and starts the minikube cluster.
set -euo pipefail

PROFILE="${PROFILE:-clinetest}"
NAMESPACE="clinetest"
# up.sh lives in iac/backend/scripts, so the repository root is three levels up.
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../../.." && pwd)"
IAC_DIR="${ROOT_DIR}/iac/backend"
TMP_DIR="$(mktemp -d)"

cleanup() { rm -rf "${TMP_DIR}"; }
trap cleanup EXIT

log() { printf '\033[0;36m==>\033[0m %s\n' "$*"; }
die() { printf '\033[0;31mERROR:\033[0m %s\n' "$*" >&2; exit 1; }

command -v minikube >/dev/null || die "minikube not found"
command -v kubectl >/dev/null || die "kubectl not found"
command -v docker >/dev/null || die "docker not found"

# 1. Cluster ---------------------------------------------------------------
if minikube status -p "${PROFILE}" 2>/dev/null | grep -q "host: Running"; then
  log "using existing minikube profile '${PROFILE}'"
else
  log "starting minikube profile '${PROFILE}' (this takes a few minutes)"
  minikube start \
    -p "${PROFILE}" \
    --driver docker \
    --cpus 4 \
    --memory 7168 \
    --addons ingress,metallb
fi

# 2. Images ----------------------------------------------------------------
# Built with the host docker daemon and then side-loaded into minikube.
# `minikube image build` is avoided because it cannot resolve /mnt/c paths.
build_and_load() {
  local dockerfile="$1" image="$2"

  log "building ${image}"
  docker build \
    --file "${IAC_DIR}/docker/${dockerfile}" \
    --tag "${image}" \
    "${ROOT_DIR}"

  log "loading ${image} into minikube"
  docker save "${image}" -o "${TMP_DIR}/image.tar"
  minikube image load -p "${PROFILE}" "${TMP_DIR}/image.tar"
  rm -f "${TMP_DIR}/image.tar"
}

build_and_load "Dockerfile.api" "clinetest-api:local"
build_and_load "Dockerfile.migrator" "clinetest-migrator:local"

# 3. Manifests -------------------------------------------------------------
# The namespace must exist before any namespaced object is created.
log "applying manifests"
kubectl apply -f "${IAC_DIR}/cluster/00-namespace.yaml"
kubectl apply -f "${IAC_DIR}/cluster/05-secret.yaml"
kubectl apply -f "${IAC_DIR}/cluster/10-mysql.yaml"
kubectl apply -f "${IAC_DIR}/cluster/20-elasticsearch.yaml"
kubectl apply -f "${IAC_DIR}/cluster/30-localstack.yaml"

# 4. ConfigMap from the compose init script (single source of truth) --------
# Built from the same file that docker-compose mounts, so the bucket
# definition cannot drift between the two environments.
log "creating localstack-init configmap from docker/localstack/init/ready.d"
kubectl create configmap localstack-init \
  --namespace "${NAMESPACE}" \
  --from-file=01-create-posters-bucket.sh="${ROOT_DIR}/docker/localstack/init/ready.d/01-create-posters-bucket.sh" \
  --dry-run=client -o yaml | kubectl apply -f -

# LocalStack reads /etc/localstack/init/ready.d only on first start, so the
# StatefulSet has to roll after the ConfigMap exists.
kubectl rollout restart statefulset/localstack --namespace "${NAMESPACE}"

log "waiting for stateful workloads to become ready"
kubectl wait --namespace "${NAMESPACE}" \
  --for=condition=Ready pod \
  -l app.kubernetes.io/part-of=clinetest \
  --timeout=900s

# 5. Migrations ------------------------------------------------------------
log "applying EF Core migrations"
# A Job spec is immutable, so remove any previous run before re-applying.
kubectl delete job clinetest-db-migrate \
  --namespace "${NAMESPACE}" --ignore-not-found
kubectl apply -f "${IAC_DIR}/cluster/40-migrate-job.yaml"
kubectl wait --namespace "${NAMESPACE}" \
  --for=condition=complete job/clinetest-db-migrate \
  --timeout=600s

# 6. API -------------------------------------------------------------------
log "deploying API"
kubectl apply -f "${IAC_DIR}/cluster/50-api.yaml"
kubectl rollout status deployment/clinetest-api \
  --namespace "${NAMESPACE}" --timeout=300s

# 7. Load balancer ---------------------------------------------------------
# A LoadBalancer Service gets a real EXTERNAL-IP when an address pool exists.
# On minikube the bundled MetalLB addon ships without its CRDs, so fall back to
# the NodePort that Kubernetes assigns instead of waiting for an IP forever.
log "resolving the load balancer endpoint"
EXTERNAL_IP=""
NODE_PORT=""
for _ in $(seq 1 60); do
  EXTERNAL_IP="$(kubectl get svc clinetest-api-lb \
    --namespace "${NAMESPACE}" \
    -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || true)"
  NODE_PORT="$(kubectl get svc clinetest-api-lb \
    --namespace "${NAMESPACE}" \
    -o jsonpath='{.spec.ports[0].nodePort}' 2>/dev/null || true)"

  if [ -n "${EXTERNAL_IP}" ]; then
    break
  fi

  if [ -n "${NODE_PORT}" ]; then
    EXTERNAL_IP="$(minikube ip -p "${PROFILE}"):${NODE_PORT}"
    break
  fi

  sleep 5
done

echo
log "cluster is up"
printf '  API:        http://%s/api/movies\n' "${EXTERNAL_IP}"
printf '  Swagger:    not exposed (ASPNETCORE_ENVIRONMENT=Production)\n\n'
log "open a tunnel for browser access with:"
log "  minikube service clinetest-api-lb -n ${NAMESPACE} -p ${PROFILE}"
log "tear down with: iac/backend/scripts/down.sh"