#!/usr/bin/env bash
# Deletes the cluster. Pass --keep-cluster to only remove the workloads.
set -euo pipefail

PROFILE="${PROFILE:-clinetest}"
KEEP_CLUSTER=false

if [ "${1:-}" = "--keep-cluster" ]; then
  KEEP_CLUSTER=true
fi

if [ "${KEEP_CLUSTER}" = true ]; then
  command -v kubectl >/dev/null || { echo "kubectl not found" >&2; exit 1; }
  echo "==> deleting namespace clinetest"
  kubectl delete namespace clinetest --ignore-not-found --wait=false
  echo "namespace deleted (PersistentVolumeClaims are removed with it)"
else
  echo "==> deleting minikube profile '${PROFILE}'"
  minikube delete -p "${PROFILE}"
  echo "cluster deleted"
fi