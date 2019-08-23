#!/usr/bin/env bash
[[ $TRAVIS == true ]] && export HIDE_PULL_PROGRESS=true
bash -e ./prepare-db-servers.sh
source ./prepare-db-servers.generated.sh
dotnet test -v:m -c Release

