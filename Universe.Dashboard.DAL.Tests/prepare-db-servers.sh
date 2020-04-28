#!/usr/bin/env bash
set -e

if [[ -n "${HIDE_PULL_PROGRESS:-}" ]]; then hide_pull=">/dev/null"; fi

function run_sql_server() {
  name="$1"
  image="$2"
  port="$3"
  exists=false; sudo docker logs "$name" >/dev/null 2>&1 && echo The SQL Server $name already exists && exists=true && printf "Startings ... "; sudo docker start $name 2>/dev/null || true
  [[ $exists == false ]] && (echo Creating SQL Server $name container using $image; eval "sudo docker pull $image $hide_pull"; sudo docker run -d --name $name -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=`1qazxsw2' -p $port:1433 $image ) || true
}
run_sql_server 'sql-2017-for-tests' 'microsoft/mssql-server-linux:2017-latest' 1434
# run_sql_server 'sql-2019-for-tests' 'mcr.microsoft.com/mssql/server:2019-CTP3.2-ubuntu' 1435
# run_sql_server 'sql-2019-for-tests' 'mcr.microsoft.com/mssql/server:2019-RC1-ubuntu' 1435
run_sql_server 'sql-2019-for-tests' 'mcr.microsoft.com/mssql/server:2019-latest' 1435




export MYSQL_TEST_DB=W3Top MYSQL_ROOT_PASSWORD=pass
url=https://raw.githubusercontent.com/devizer/glist/master/install-5-mysqls-for-tests-V2.sh; (wget -q -nv --no-check-certificate -O - $url 2>/dev/null || curl -skSL $url) | bash

export POSTGRESQL_DB=W3Top POSTGRESQL_PASS=passw0rd POSTGRESQL_USER=postgres WAIT_TIMEOUT=30
url=https://raw.githubusercontent.com/devizer/glist/master/install-7-postres-for-tests.sh; (wget -q -nv --no-check-certificate -O - $url 2>/dev/null || curl -skSL $url) | bash

file=prepare-db-servers.generated.sh
printf "" > $file

echo '
MSSQL_TEST_SERVER_2017="Server=localhost,1434;User=sa;Password=\`1qazxsw2;Pooling=false"
export MSSQL_TEST_SERVER_2017
' >> $file

# what the hell
if true || [[ "${TRAVIS:-}" != "true" ]] || [[ "$(lsb_release -cs)" == "trusty" ]]; then
echo '
MSSQL_TEST_SERVER_2019="Server=localhost,1435;User=sa;Password=\`1qazxsw2;Pooling=false"
export MSSQL_TEST_SERVER_2019
' >> $file
fi


# postgres 8.4 doesnt work on core 2 duo
has_avx=`lscpu | grep 'avx'` || true
# [ -n "$has_avx" ] && echo "AVX is presented" || echo "AVX is absent"

echo '
# postgres' >> $file
for p in {54321..54328}; do
  name="PGSQL_TEST_SERVER_$p"
  line="$name=\"Host=localhost;Port=$p;Database=postgres;Username=postgres;Password=$POSTGRESQL_PASS;Timeout=15;Pooling=false;\""
  if [[ $p -ne 54328 ]] || [[ -n "$has_avx" ]]; then
    echo "$line" >> $file
    echo "export $name" >> $file
  fi
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
echo "Completed: $file"