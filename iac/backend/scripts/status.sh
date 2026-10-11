#!/usr/bin/env bash
# Prints the current state of the cluster.
set -euo pipefail

PROFILE="${PROFILE:-clinetest}"
NAMESPACE="${NAMESPACE:-clinetest}"

echo "==> pods"
kubectl get pods --namespace "${NAMESPACE}" -o wide

echo
echo "==> services"
kubectl get svc --namespace "${NAMESPACE}"

echo
echo "==> external IP"
kubectl get svc clinetest-api-lb \
  --namespace "${NAMESPACE}" \
  -o jsonpath='{.status.loadBalancer.ingress[0].ip}' 2>/dev/null || true
echo

echo
echo "==> PVCs"
kubectl get pvc --namespace "${NAMESPACE}"