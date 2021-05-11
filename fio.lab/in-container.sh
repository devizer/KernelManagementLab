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
set -o pipefail
short="1M 1 0" long="1G 3 3"
# ========== DURATIN ============
duration="$short"
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
    # ? --status-interval=1
    export FILE_IO_BENCHMARK_OPTIONS="--eta=always --time_based"
    export FILE_IO_BENCHMARK_DUMP_FOLDER=/out/dumps
    # File-IO-Benchmark "CONTAINER" $(pwd) $duration | tee /out/Benchmark.log
    exit_code=$?
    gzip -9 /out/Benchmark.log
    fio --enghelp > /out/enghelp-show-engine-list.log
    Say "EXIT CODE of File-IO-Benchmark: $exit_code"
    # cd $FILE_IO_BENCHMARK_DUMP_FOLDER; cd *
    popd >/dev/null
elif [[ -s /usr/local/bin/fio ]]; then
  # 2.0 & 2.1
  mkdir -p /tmp/fio; 
  rm -rf /tmp/fio/*
  for f in /usr/local/bin/fio /usr/local/man/man1/fio.1; do
    cp $f /tmp/fio
    rm -f $f
  done 
  pushd /tmp/fio >/dev/null
    strip fio
    tar czf /out/fio-stripped.tar.gz .
    echo ""
    Say "Testing fio ..."
    export PATH="$(pwd):$PATH"
    export FILE_IO_BENCHMARK_OPTIONS="--eta=always --time_based"
    export FILE_IO_BENCHMARK_DUMP_FOLDER=/out/dumps
    # File-IO-Benchmark "CONTAINER" $(pwd) $duration | tee /out/Benchmark.log
    exit_code=$?
    gzip -9 /out/Benchmark.log
    fio --enghelp > /out/enghelp-show-engine-list.log
    Say "EXIT CODE of File-IO-Benchmark: $exit_code"
  popd >/dev/null
  
fi
