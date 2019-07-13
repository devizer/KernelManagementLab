### Install mysql

echo mysql-server-5.5 mysql-server/root_password password root | sudo debconf-set-selections
echo mysql-server-5.5 mysql-server/root_password_again password root | sudo debconf-set-selections
sudo apt-get -y install mysql-server
mysql -u root -p'root' -e "SHOW VARIABLES LIKE \"%version%\";"
mysql -u root -p'root' -e "CREATE USER 'admin'@'%' IDENTIFIED BY 'admin'; GRANT ALL PRIVILEGES ON *.* TO 'admin'@'%' WITH GRANT OPTION;"
mysql -u root -p'root' -e "CREATE DATABASE w3top1 CHARACTER SET utf8 COLLATE utf8_unicode_ci;"

# redhat 6
# Install mysql server and configure autostart and root's password
# MySQL 5.7 for redhat/centos 6 
sudo rpm -Uvh https://repo.mysql.com/mysql57-community-release-el6-11.noarch.rpm
sudo yum install -y mysql-community-server
sudo mysql_secure_installation
user=root; password=""

# MySQL 5.1 for redhat/centos 6 
yum install -y tar sudo
sudo yum install -y mysql-server mysql
sudo chkconfig mysqld on
sudo /etc/init.d/mysqld start
sudo mysqladmin -u root password secret


# Create empty database w3top ... 
# w3top relies on default DB's charset and collation
user=root; password=secret
mysql -u $user -p"$password" -e "DROP DATABASE w3top;" 2>/dev/null || true
mysql -u $user -p"$password" -e "CREATE DATABASE w3top CHARACTER SET utf8 COLLATE utf8_unicode_ci;"
# ... and grant access to it for user w3top identified by password D0tN3t;42
mysql -u $user -p"$password" -e "DROP USER 'w3top'@'localhost';" 2>/dev/null  || true
mysql -u $user -p"$password" -e "CREATE USER 'w3top'@'localhost' IDENTIFIED BY 'D0tN3t;42'; GRANT ALL PRIVILEGES ON w3top.* TO 'w3top'@'localhost' WITH GRANT OPTION;"
mysql -u w3top -p'D0tN3t;42' -e "SHOW VARIABLES LIKE \"%version%\";" w3top

# point w3top to newly created empty database.
export MYSQL_DATABASE='Server=localhost;Database=w3top;Port=3306;Uid=w3top;Pwd="D0tN3t;42";Connect Timeout=20;Pooling=false;'
export HTTP_PORT=5050
export RESPONSE_COMPRESSION=True
export INSTALL_DIR=/opt/w3top
script=https://raw.githubusercontent.com/devizer/w3top-bin/master/install-w3top-service.sh
(wget -q -nv --no-check-certificate -O - $script 2>/dev/null || curl -ksSL $script) | bash
sudo journalctl -fu w3top || cat /tmp/w3top.log




CREATE TABLE `T2` (`Id` int NOT NULL AUTO_INCREMENT, `CreatedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP, PRIMARY KEY (`Id`));

