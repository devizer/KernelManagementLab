#!/usr/bin/env bash
echo "MYSQL_TEST_CONNECTION: [$MYSQL_TEST_CONNECTION]"
bash -e ./prepare-db-servers.sh
source ./prepare-db-servers.generated.sh
dotnet test -v:m -c Release

