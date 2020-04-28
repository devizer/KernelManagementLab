sudo apt-get install libreadline-dev make -y
url=https://ftp.postgresql.org/pub/source/v12.2/postgresql-12.2.tar.gz
file=$(basename $url)
work=$HOME/build/postgresql-src
mkdir -p $work
pushd $work
rm -rf *
wget -O _$file $url
tar xzf _$file
rm _$file
cd postgres*
./configure --prefix=/usr/local
time make -j5
sudo make -C src/bin install
sudo make -C src/include install
sudo make -C src/interfaces install
sudo make -C doc install
popd 
rm -rf $work
