test -s /etc/os-release && source /etc/os-release

function try_and_retry () {
  eval "$*" || eval "$*" || eval "$*"
}

function prepare_debian() {
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

try_and_retry apt-get update -q;
apt-cache policy fio
echo ""
apt-cache policy libaio-dev
echo ""
echo "AIO support packages"
apt-cache search "(fio|libaio)" 
echo "";
# apt-get install libaio-dev -y -qq
# build-essential
# also depends on []zlib1g zlib1g-dev] but not included
# removed: libncurses5-dev libncurses5 libncursesw5-dev libncursesw5
cmd="apt-get install --no-install-recommends libc6-dev gcc build-essential autoconf autoconf make -y -q"
eval $cmd || eval $cmd || eval $cmd

}

function prepare_centos() {
if [[ ! -s /etc/os-release ]]; then
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
fi

try_and_retry yum makecache
try_and_retry yum install gcc make -y;
# echo ""; echo "Installing libaio-dev"
# yum install libaio-devel -y || yum install libaio-devel -y || yum install libaio-devel -y
} # centos

function prepare_centos_stream() {
  try_and_retry yum makecache
  try_and_retry yum install centos-release-stream -y; 
  try_and_retry yum distro-sync -y;
  try_and_retry yum install gcc make -y;
}