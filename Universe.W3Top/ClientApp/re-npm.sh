#!/bin/bash
set -e
./clean.sh
time npm install
time npm run build

