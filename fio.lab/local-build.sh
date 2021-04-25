cd /build/
work=/fio-src
mkdir -p $work
cd $work
tar xzf /build/fio-current.tar.gz
./configure --prefix=/usr/local/fio
cpus=$(cat /proc/cpuinfo | grep -E '^(P|p)rocessor' | wc -l)
make -j${cpus}
make install
