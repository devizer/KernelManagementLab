#!/usr/bin/env bash
echo "MYSQL_TEST_CONNECTION: [$MYSQL_TEST_CONNECTION]"
dotnet test -v:m -c Release

