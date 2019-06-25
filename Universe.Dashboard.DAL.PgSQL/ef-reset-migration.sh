#!/usr/bin/env bash
# set -e
rm -rf Migrations
dotnet build

commands=( \
  "drop database if exists w3top;" \
  "drop user if exists w3top;" \
  "create database w3top;" \
  "create user w3top;" \
  "ALTER USER w3top WITH PASSWORD 'pass';" \
  "grant all on DATABASE w3top to w3top;" \
)
for sql in "${commands[@]}"; do
  pushd /tmp >/dev/null
  sudo -u postgres psql -q -t -c "$sql"
  popd >/dev/null
done

export PGSQL_DATABASE="Host=localhost;Database=w3top;Username=w3top;Password=pass"
dotnet ef migrations add Initial_PG
dotnet ef database update

# dotnet ef database update;


