#!/usr/bin/env bash

mysqldump --no-data -u admin -p'admin' w3top > bin/w3top-runtime-no-data.sql
mysqldump -u admin -p'admin' w3top > bin/w3top-runtime-WITH-DATA.sql

mysqldump --no-data -u admin -p'admin' w3top_tests > bin/w3top-TESTS-runtime-no-data.sql
mysqldump -u admin -p'admin' w3top_tests > bin/w3top-TESTS-runtime-WITH-DATA.sql

