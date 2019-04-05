#!/bin/bash
set -e
./clean.sh
./inc-build.sh
time yarn install
time yarn build

