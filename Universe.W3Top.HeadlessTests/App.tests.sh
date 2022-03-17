#!/usr/bin/env bash

function prepare_disk_benchmark_history_on_travis() {
curl -i -H "Accept: application/json" \
    -X POST -d '{mountPath: "/", workingSet: "128", randomAccessDuration: "2", disableODirect: false, blockSize: 4096, threads: 16}' \
    http://localhost:5050/api/benchmark/disk/start-disk-benchmark
}

test ! -d node_modules && yarn install

export W3TOP_APP_URL="http://localhost:${PORT:-5055}"
yarn test

