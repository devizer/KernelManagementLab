#!/usr/bin/env bash
bash PgSQL-reset-db.sh
export PGSQL_DATABASE="Host=localhost;Database=w3top;Username=w3top;Password=pass"
dotnet ef database update
sudo -u postgres pg_dump w3top > bin/w3top-after-migrations.sql
