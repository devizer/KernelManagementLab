#!/usr/bin/env bash
export CFLAGS="-O2"
export CXXFLAGS="-O2"
echo "LIBC: $(ldd --version)"
echo ""

echo "uname -m: [$(uname -m)], uname -p: [$(uname -p)]"
echo ""

source /etc/os-release

export DEBIAN_FRONTEND=noninteractive

cd fio* || true
echo ""
echo "CURRENT DIRECTORY: [$(pwd)]. Building"
export CFLAGS="-O2" CXXFLAGS="-O2" CPPFLAGS="-O2"
./configure --prefix=/usr/local/fio
cpus=$(cat /proc/cpuinfo | grep -E '^(P|p)rocessor' | wc -l)
cpus=$((cpus + 1))
make -j${cpus} && make install
mkdir -p /out
rm -rf /out/*
if [[ -d /usr/local/fio ]]; then
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
    Say "About *sync* engine"
    bin/fio --enghelp=sync
    echo ""
    Say "About *posixaio* engine"
    bin/fio --enghelp=posixaio
    echo ""
    Say "About *libaio* engine"
    bin/fio --enghelp=libaio
    echo ""
    Say "fio dependencies are below"
    ldd bin/fio
    echo ""
    Say "Testing fio ..."
    export PATH="$(pwd)/bin:$PATH"
    export FILE_IO_BENCHMARK_OPTIONS="--eta=always --time_based"
    File-IO-Benchmark "CONTAINER" $(pwd) 1G 3 3 | tee /out/Benchmark.log
    gzip -9 /out/Benchmark.log
    Say "EXIT CODE of File-IO-Benchmark: $?"
    popd >/dev/null
fi
