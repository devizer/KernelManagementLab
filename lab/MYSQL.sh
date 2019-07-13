# Travis 14.04: 5.5.62
# Travis 14.04: 5.7.26

Docker:
5.6, 5.7, 8,0

# 5.1: https://sonnguyen.ws/install-mysql-5-1-ubuntu-14/

https://hub.docker.com/r/vsamov/mysql-5.1.73
https://hub.docker.com/r/mysql/mysql-server/

set -e
set -u

function wait_for() {
  n=$1
  p=$2
  printf "\nWaiting for $n on localhost @ $p ...."
  counter=0; total=30; started=""
  while [ $counter -lt $total ]; do
    counter=$((counter+1));
    mysql --protocol=TCP -h localhost -u root -p'D0tN3t' -P $p -e "Select 1;" 2>/dev/null 1>&2 && started="yes" || true
    if [ -n "$started" ]; then printf " OK\n"; break; else (sleep 1; printf $counter"."); fi
  done
}

images=("vsamov/mysql-5.1.73" "mysql/mysql-server:5.5" "mysql/mysql-server:5.6" "mysql/mysql-server:5.7" "mysql/mysql-server:8.0")
names=("mysql-5.1" "mysql-5.5" "mysql-5.6" "mysql-5.7" "mysql-8.0")
count=${#images[@]}
echo "COUNT: $count"
for (( i=0; i<$count; i++ )); do
  image=${images[$i]}
  name=${names[$i]}
  echo "[$(($i+1)) / $count] container: $name, image: $image"
  time docker pull "$image"
  port=$((3306+1+$i));
  cmd="docker run --name $name -e MYSQL_ROOT_HOST=% -e MYSQL_ROOT_PASSWORD=D0tN3t -e MYSQL_DATABASE=w3top -d -p $port:3306 $image"
  echo ""; echo $cmd
  eval "$cmd" || true
  # sleep 8; docker logs $name
  wait_for "$name" "$port"
  mysql --protocol=TCP -h localhost -u root -p'D0tN3t' -P $port -e "Select version() as \`MySQL Server at $port port\`; show databases;"
  echo ""
done

# LIST MySQL server processes
ps aux | grep mysqld | grep -v grep

# LIST MySQL docker images
docker images -a |  grep -E "mysql-"

# REMOVE all containers and mysql images
docker rm -f $(docker ps -a -q); 
docker images -a |  grep -E "mysql-" | awk '{print $3}' | xargs docker rmi -f

docker rm -f $(docker ps -a -q); 
docker images -a |  grep -E "postgres-" | awk '{print $3}' | xargs docker rmi -f

#########################################################################################################
sudo docker rm -f $(docker ps -a -q); sudo docker rmi $(docker images -a -q); sudo docker volume prune -f

docker images -a | awk '{print $3}' | xargs docker rmi -f


docker run -d --name mysql-5.1.73 -p 3307:3306 -e MYSQL_ROOT_PASSWORD=[password] vsamov/mysql-5.1.73:latest

docker rm -f $(docker ps -a -q); 
docker images -a | awk '{print $3}' | xargs docker rmi -f
