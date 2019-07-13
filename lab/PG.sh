# 8.4: https://github.com/psycopg/psycopg2/issues/430
# 9 amd above: https://hub.docker.com/_/postgres
POSTGRESQL_DB=APP42
POSTGRESQL_PASS=pass
POSTGRESQL_USER=postgres

vars="-e POSTGRESQL_USER=$POSTGRESQL_USER -e POSTGRESQL_PASS=$POSTGRESQL_PASS -e POSTGRESQL_DB=$$POSTGRESQL_DB -e POSTGRES_USER=$POSTGRESQL_USER -e POSTGRES_PASS=$POSTGRESQL_PASS -e POSTGRES_DB=$POSTGRESQL_DB"
eval "docker run --name postgres-12  $vars -p 54328:5432 -d postgres:12"
eval "docker run --name postgres-11  $vars -p 54327:5432 -d postgres:11.4"
eval "docker run --name postgres-10  $vars -p 54326:5432 -d postgres:10.9"
eval "docker run --name postgres-9.6 $vars -p 54325:5432 -d postgres:9.6"
eval "docker run --name postgres-9.5 $vars -p 54324:5432 -d postgres:9.5"
eval "docker run --name postgres-9.4 $vars -p 54323:5432 -d postgres:9.4"
eval "docker run --name postgres-9.1 $vars -p 54322:5432 -d postgres:9.1"
eval "docker run --name postgres-8.4 $vars -p 54321:5432 -d postgres:8.4"

for port in {54321..54328}; do
  printf "checking port $port ...";
  # sleep 1
  cmd1='PGPASSWORD=pass psql -t -h localhost -p '$port' -U postgres -q -c "select version();"';
  v1="unknown"; v1=$(eval $cmd1); v1="${v1## }";
  cmd2='PGPASSWORD=pass psql -t -h localhost -p '$port' -U postgres -q -c "show server_version;"';
  v2="unknown"; v2=$(eval $cmd2); v2="${v2## }";
  printf "\r$port: [$v2] $v1\n";

  for sql in "drop database app_utf;" "create database app_utf with encoding 'LATIN1';" "create user user42;" "create database app1;" "grant all on DATABASE app1 to user42;" "drop database app1" "drop user user42"; do
    echo SQL: $sql
    cmd_test1='PGPASSWORD=pass psql -t -h localhost -p '$port' -U postgres -q -c "'$sql'"';
    eval $cmd_test1
    cmd_test2='PGPASSWORD=pass psql -t -h localhost -p '$port' -U postgres -q -l';
    eval $cmd_test2
  done
  # SELECT * FROM pg_catalog.pg_tables;
done

# 8.4: Encoding: SQL_ASCII, Collation and Type: en_US.UTF-8
# 9.1+: Encoding: UTF8, Collation and Type: C

name="nahuel/postgresql-8.4"
name="postgres:9.1"
echo $(basename $name)
n="${name/:/-}"
echo $n



# on centos/rhel 6, 8.4.20
sudo yum install -y postgresql postgresql-server postgresql-contrib
sudo service postgresql initdb
echo '
local  all  all                ident
host   all  all  127.0.0.1/32  md5
host   all  all  ::1/128       md5
' | sudo tee /var/lib/pgsql/data/pg_hba.conf
sudo service postgresql restart
sudo -u postgres psql -q -t -c "Select Version();"

sudo -u postgres psql -c "CREATE ROLE admin WITH SUPERUSER LOGIN PASSWORD 'pass';"
PGPASSWORD=pass psql -t -h localhost -p 5432 -U admin -q -c "select version();" -d postgres
echo Ver: $(eval 'PGPASSWORD=pass psql -t -h localhost -p 5432 -U admin -q -c "select version();" -d postgres')

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

PGPASSWORD=pass psql -t -h localhost -p 5432 -U w3top -q -c "select 'Hello, ' || current_user;" -d w3top
export MYSQL_DATABASE=
export PGSQL_DATABASE="Host=localhost;Port=5432;Database=w3top;Username=w3top;Password=pass;Timeout=15;Pooling=false;"
