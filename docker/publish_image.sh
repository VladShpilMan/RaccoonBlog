#!/bin/bash
set -euo pipefail

APP_IMAGE_LOCAL="ayendeblog-website-test:latest"

TEST_REGION="eu-central-1"
TEST_REGISTRY="846865426872.dkr.ecr.eu-central-1.amazonaws.com/ravendb/ayendeblog-website-test"
TEST_ECR="${TEST_REGISTRY}/ravendb/ravendb/ayendeblog-website-test:latest"

PROD_REGION=""
PROD_REGISTRY=""
PROD_ECR=""

echo "Build Docker image"
docker build -t "${APP_IMAGE_LOCAL}" .

if [[ -n "${PRODUCTION:-}" ]]; then
  echo "Login into Docker repo - PROD"

  aws ecr get-login-password --region "${PROD_REGION}" \
    | docker login --username AWS --password-stdin "${PROD_REGISTRY}"


  TARGET_ECR="${PROD_ECR}"
else
  echo "Login into Docker repo - TEST"

  aws ecr get-login-password --region "${TEST_REGION}" \
    | docker login --username AWS --password-stdin "${TEST_REGISTRY}"

  TARGET_ECR="${TEST_ECR}"
fi

echo "Tag image -> ${TARGET_ECR}"
docker tag "${APP_IMAGE_LOCAL}" "${TARGET_ECR}"

echo "Push image -> ${TARGET_ECR}"
docker push "${TARGET_ECR}"

echo "Done."