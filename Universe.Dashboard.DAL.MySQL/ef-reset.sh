#!/usr/bin/env bash
set -e
rm -rf Migrations
dotnet build
mysql -u admin -p'admin' -e "drop database w3top_b1;"
dotnet ef migrations add Initial 
dotnet ef database update;

mysqldump --no-data -u admin -p'admin' w3top_b1 > bin/dump.sql

