#!/usr/bin/env bash
user=admin; password=admin
mysql -u $user -p"$password" -e "DROP DATABASE w3top;" 2>/dev/null || true
mysql -u $user -p"$password" -e "DROP USER 'w3top'@'%';" 2>/dev/null  || true
mysql -u $user -p"$password" -e "CREATE DATABASE w3top CHARACTER SET utf8 COLLATE utf8_unicode_ci;"
mysql -u $user -p"$password" -e "CREATE USER 'w3top'@'%' IDENTIFIED BY 'w3top'; GRANT ALL PRIVILEGES ON w3top.* TO 'w3top'@'%' WITH GRANT OPTION;"

mysql -u $user -p"$password" -e "DROP DATABASE w3top_tests;" 2>/dev/null || true
mysql -u $user -p"$password" -e "CREATE DATABASE w3top_tests CHARACTER SET utf8 COLLATE utf8_unicode_ci;"
mysql -u $user -p"$password" -e "GRANT ALL PRIVILEGES ON w3top_tests.* TO 'w3top'@'%' WITH GRANT OPTION;"
export MYSQL_DATABASE="Server=localhost;Database=w3top;Port=3306;Uid=w3top;Pwd=w3top;Connect Timeout=5;Pooling=false;"
