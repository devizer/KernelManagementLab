#!/bin/bash
set -e
./clean.sh
./inc-build.sh
time npm install
time npm run build

