#!/usr/bin/env bash
set -e
set -u

./build-w3-dashboard.sh deploy_to_gae
bash publish-public.sh