#!/usr/bin/env bash
[[ $TRAVIS == true ]] && export HIDE_PULL_PROGRESS=true
bash -e ./prepare-db-servers.sh
source  ./prepare-db-servers.generated.sh
dotnet test -f netcoreapp2.2 -v:m -c Release | cat

exit;

cd bin/Release/netcoreapp*/bin
mkdir -p ~/build-artifacts
cp -r * ~/build-artifacts

exit;

count=5
names=("mysql-5.1" "mysql-5.5" "mysql-5.6" "mysql-5.7" "mysql-8.0")
MYSQL_ROOT_PASSWORD=pass
# select distinct table_schema from information_schema.tables where table_schema not in ('mysql','information_schema');
for (( i=0; i<$count; i++ )); do
  name=${names[$i]} port=$((3306+1+$i));
  ver=$(docker exec -t $name sh -c "MYSQL_PWD=\"$MYSQL_ROOT_PASSWORD\" mysql -s -N --protocol=TCP -h localhost -u root -P 3306 -e 'Select version();' 2>&1");
  echo "MySQL server version on port [$port] is [$ver]";
  mkdir -p bin/databases
  docker exec -t $name sh -c "MYSQL_PWD=\"$MYSQL_ROOT_PASSWORD\" mysqldump --protocol=TCP -h localhost -u root -P 3306 mysql" > bin/databases/mysql-${ver}.sql
  # echo "";
done
