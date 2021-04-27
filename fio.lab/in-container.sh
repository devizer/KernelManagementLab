#!/usr/bin/env bash
export CFLAGS="-O2"
export CXXFLAGS="-O2"
echo "LIBC: $(ldd --version)"
echo ""

source /etc/os-release
  if [[ $ID == debian ]] && [[ $VERSION_ID == 8 ]]; then
    rm -f /etc/apt/sources.list.d/backports* || true
    echo '
deb http://deb.debian.org/debian jessie main
deb http://security.debian.org jessie/updates main
' > /etc/apt/sources.list
    fi

  if [[ $ID == debian ]] && [[ $VERSION_ID == 7 ]]; then
    rm -f /etc/apt/sources.list.d/backports* || true
    echo '
deb http://archive.debian.org/debian wheezy main contrib
# deb http://security.debian.org/debian-security wheezy/updates main
deb http://archive.debian.org/debian-security wheezy/updates main
# deb http://archive.debian.org/debian wheezy-updates main

' > /etc/apt/sources.list
    fi

# NO Install-Recommends
echo '
APT::Install-Recommends "0";
APT::NeverAutoRemove:: ".*";

Acquire::Check-Valid-Until "0";
APT::Get::Assume-Yes "true";
APT::Get::AllowUnauthenticated "true";
Acquire::AllowInsecureRepositories "1";
Acquire::AllowDowngradeToInsecureRepositories "1";

Acquire::CompressionTypes::Order { "gz"; };
APT::Compressor::gzip::CompressArg:: "-1";
APT::Compressor::xz::CompressArg:: "-1";
APT::Compressor::bzip2::CompressArg:: "-1";
APT::Compressor::lzma::CompressArg:: "-1";
' > /etc/apt/apt.conf.d/99Z_Custom

export DEBIAN_FRONTEND=noninteractive

if [[ $(command -v apt-get 2>/dev/null) != "" ]]; then
    apt-get update -qq || apt-get update -qq || apt-get update
    apt-cache policy fio
    echo ""
    apt-cache policy libaio-dev
    echo ""
    echo "AIO support packages"
    apt-cache search "(fio|libaio)" 
    echo "";
    apt-get install libaio-dev -y -qq
    # build-essential
    # also depends on []zlib1g zlib1g-dev] but not included
    # removed: libncurses5-dev libncurses5 libncursesw5-dev libncursesw5
    cmd="apt-get install --no-install-recommends libc6-dev gcc gettext build-essential autoconf autoconf make -y -q"
    eval $cmd || eval $cmd || eval $cmd
fi

if [[ $(command -v yum 2>/dev/null) != "" ]]; then
cat <<-'EOF' > /etc/yum.repos.d/CentOS-Base.repo
[C6.10-base]
name=CentOS-6.10 - Base
baseurl=http://vault.centos.org/6.10/os/$basearch/
gpgcheck=1
gpgkey=file:///etc/pki/rpm-gpg/RPM-GPG-KEY-CentOS-6
enabled=1
metadata_expire=never

[C6.10-updates]
name=CentOS-6.10 - Updates
baseurl=http://vault.centos.org/6.10/updates/$basearch/
gpgcheck=1
gpgkey=file:///etc/pki/rpm-gpg/RPM-GPG-KEY-CentOS-6
enabled=1
metadata_expire=never

[C6.10-extras]
name=CentOS-6.10 - Extras
baseurl=http://vault.centos.org/6.10/extras/$basearch/
gpgcheck=1
gpgkey=file:///etc/pki/rpm-gpg/RPM-GPG-KEY-CentOS-6
enabled=1
metadata_expire=never

[C6.10-contrib]
name=CentOS-6.10 - Contrib
baseurl=http://vault.centos.org/6.10/contrib/$basearch/
gpgcheck=1
gpgkey=file:///etc/pki/rpm-gpg/RPM-GPG-KEY-CentOS-6
enabled=0
metadata_expire=never

[C6.10-centosplus]
name=CentOS-6.10 - CentOSPlus
baseurl=http://vault.centos.org/6.10/centosplus/$basearch/
gpgcheck=1
gpgkey=file:///etc/pki/rpm-gpg/RPM-GPG-KEY-CentOS-6
enabled=0
metadata_expire=never
EOF
    yum makecache || yum makecache >/dev/null 2>&1 || yum makecache
    yum install gcc make gettext -y || yum install gcc gettext -y || yum install gcc gettext -y;
    echo ""
    echo "Installing libaio"
    yum install libaio -y || yum install libaio -y || yum install libaio -y
    echo ""
    echo "Installing libaio-dev"
    yum install libaio-devel -y || yum install libaio-devel -y || yum install libaio-devel -y
fi

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
    pushd /usr/local/fio
    tar czf /out/fio-distribution.tar.gz .
    cd bin; tar czf /out/fio.tar.gz fio; cd ..
    echo "STRIPPING"
    strip bin/*
    tar czf /out/fio-distribution-stripped.tar.gz .
    cd bin; tar czf /out/fio-stripped.tar.gz fio; cd ..
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
    File-IO-Benchmark "CONTAINER" $(pwd) 1M 1 0;
    echo "EXIT CODE of File-IO-Benchmark: $?"
    popd >/dev/null
fi
