#!/usr/bin/env bash
set -e
rm -rf Migrations
dotnet build
mysql -u admin -p'admin' -e "drop database w3top_b1;" || true
dotnet ef migrations add Initial 


# dotnet ef database update;

sql="CREATE TABLE UpgradeHistory ( MigrationId varchar(150) NOT NULL, ProductVersion varchar(32) NOT NULL, PRIMARY KEY (MigrationId) ) CHARSET=utf8;"
mysql -u admin -p'admin' -e "create database w3top_b1;"
# mysql -u admin -p'admin' -e "$sql" w3top_b1


dotnet run || true

mysqldump --no-data -u admin -p'admin' w3top_b1 > bin/dump-via-runtime.sql
mysqldump -u admin -p'admin' w3top_b1 > bin/dump-via-runtime-with-data.sql

