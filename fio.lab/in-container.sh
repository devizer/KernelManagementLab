#!/usr/bin/env bash
export CFLAGS="-O2"
export CXXFLAGS="-O2"
echo "LIBC: $(ldd --version)"
echo ""

source /etc/os-release

export DEBIAN_FRONTEND=noninteractive

cd fio* || true
echo ""
echo "CURRENT DIRECTORY: [$(pwd)]. Building"
export CFLAGS="-O2" CXXFLAGS="-O2" CPPFLAGS="-O2"
./configure --prefix=/usr/local/fio
cpus=$(cat /proc/cpuinfo | grep -E '^(P|p)rocessor' | wc -l)
make -j${cpus} && make install
mkdir -p /out
rm -rf /out/*
if [[ -d /usr/local/fio ]]; then
    export GZIP=-9
    pushd /usr/local/fio
    tar czf /out/fio-distribution.tar.gz .
    cd bin; 
        tar czf /out/fio.tar.gz fio; 
    cd ..

    echo "STRIPPING"
    strip bin/*
    tar czf /out/fio-distribution-stripped.tar.gz .
    cd bin; 
        tar czf /out/fio-stripped.tar.gz fio; 
    cd ..
    echo ""
    echo "About *sync* engine"
    bin/fio --enghelp=sync
    echo ""
    echo "About *posixaio* engine"
    bin/fio --enghelp=posixaio
    echo ""
    echo "About *libaio* engine"
    bin/fio --enghelp=libaio
    echo ""
    echo "fio dependencies"
    ldd bin/fio
    echo ""
    echo "Testing fio ..."
    export PATH="$(pwd)/bin:$PATH"
    export FILE_IO_BENCHMARK_OPTIONS="--eta=always --time_based"
    File-IO-Benchmark "CONTAINER" $(pwd) 1M 3 0;
    echo "EXIT CODE of File-IO-Benchmark: $?"
    popd >/dev/null
fi
