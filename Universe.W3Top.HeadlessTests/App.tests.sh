#!/usr/bin/env bash

info='to prepare data for disk benchmark history on travis-ci:
http://localhost:5050/api/benchmark/disk/start-disk-benchmark

ccept: application/json
payload: {workingSet: "128", randomAccessDuration: "2", disableODirect: false, blockSize: 4096, threads: 16}

 
curl -i -H "Accept: application/json" \
    -X POST -d '{workingSet: "128", randomAccessDuration: "2", disableODirect: false, blockSize: 4096, threads: 16}' \
    http://localhost:5050/api/benchmark/disk/start-disk-benchmark
'

yarn test

