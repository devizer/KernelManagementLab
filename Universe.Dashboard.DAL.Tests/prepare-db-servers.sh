#!/usr/bin/env bash

export MYSQL_TEST_DB=W3Top MYSQL_ROOT_PASSWORD=pass
url=https://raw.githubusercontent.com/devizer/glist/master/install-5-mysqls-for-tests-V2.sh; (wget -q -nv --no-check-certificate -O - $url 2>/dev/null || curl -skSL $url) | bash

export POSTGRESQL_DB=W3Top POSTGRESQL_PASS=pass POSTGRESQL_USER=postgres WAIT_TIMEOUT=30
url=https://raw.githubusercontent.com/devizer/glist/master/install-7-postres-for-tests.sh; (wget -q -nv --no-check-certificate -O - $url 2>/dev/null || curl -skSL $url) | bash

file=prepare-db-servers.generated.sh
printf "" > $file

echo '
# postgres' >> $file
for p in {54321..54328}; do
  name="PGSQL_TEST_SERVER_$p"
  line="$name=\"Host=localhost;Port=$p;Database=postgres;Username=postgres;Password=pass;Timeout=15;Pooling=false;\""
  echo "$line" >> $file
  echo "export $name" >> $file
done

echo '
# mysql' >> $file
for p in {3307..3311}; do
  name="MYSQL_TEST_SERVER_$p"
  line="$name=\"Server=localhost;Database=mysql;Port=$p;Uid=root;Pwd=pass;Connect Timeout=15;Pooling=false;\""
  echo "$line" >> $file
  echo "export $name" >> $file
done

chmod +x $file
