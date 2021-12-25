sudo cp /etc/apt/sources.list /etc/apt/sources.list~
sudo sed -Ei 's/^# deb-src /deb-src /' /etc/apt/sources.list
cat /etc/apt/sources.list
sudo apt-get update      
Say "Installing qemu build dependencies"
time sudo apt-get build-dep qemu -y -q
sudo apt-get install git libglib2.0-dev libfdt-dev libpixman-1-dev zlib1g-dev
sudo apt-get install ninja-build cmake -q -y
sudo apt-get install -q -y libnfs-dev libiscsi-dev

sudo apt-get install -y -q git-email
sudo apt-get install -y -q libaio-dev libbluetooth-dev libbrlapi-dev libbz2-dev
sudo apt-get install -y -q libcap-dev libcap-ng-dev libcurl4-gnutls-dev libgtk-3-dev
sudo apt-get install -y -q libibverbs-dev libjpeg8-dev libncurses5-dev libnuma-dev
sudo apt-get install -y -q librbd-dev librdmacm-dev
sudo apt-get install -y -q libsasl2-dev libsdl1.2-dev libseccomp-dev libsnappy-dev libssh2-1-dev
sudo apt-get install -y -q libvde-dev libvdeplug-dev libvte-2.90-dev libxen-dev liblzo2-dev
sudo apt-get install -y -q valgrind xfslibs-dev 


QEMU_VER=5.0.0
QEMU_VER=6.2.0 
QEMU_VER=2.12.1 
QEMU_VER=2.11.2 
Say "Downloading qemu ${QEMU_VER}"
work=$HOME/build/qemu-user-static-src
mkdir -p $work
pushd $work
rm -rf *
url=https://download.qemu.org/qemu-${QEMU_VER}.tar.bz2
file=$(basename $url)
wget -q --no-check-certificate -O _$file $url || curl -ksSL -o _$file $url
tar xjf _$file
rm _$file
cd qemu*
    # --target-list=arm-linux-user,aarch64-linux-user \

# --target-list=arm-linux-user,aarch64-linux-user
# --enable-linux-user
# --disable-avx2 \
# --static \


Say "Building qemu-arm and qemu-aarch64 using $cpus cores"
mkdir -p bin
pushd bin
export CFLAGS="-O2" CXXFLAGS="-O2"
prefix=/usr/local
../configure --target-list=arm-softmmu,arm-linux-user,aarch64-softmmu,aarch64-linux-user \
    --prefix=${prefix} \
    --disable-gtk 

set -o pipefail
cpus=$(nproc); cpus=$((cpus+1))
Say "Building qemu ${QEMU_VER}"
time make -j${cpus}
Say "Installing qemu ${QEMU_VER} to $prefix"
sudo make install
popd


# sudo ln -s -f $prefix/bin/qemu-arm /usr/bin/qemu-arm-static
# sudo ln -s -f $prefix/bin/qemu-aarch64 /usr/bin/qemu-aarch64-static

popd
rm -rf $work

Say "qemu-arm-static: $(qemu-arm-static --version | head -1)"
Say "qemu-aarch64-static: $(qemu-aarch64-static --version | head -1)"
Say "qemu-system-arm: $(qemu-system-arm --version | head -1)"
Say "qemu-system-aarch64: $(qemu-system-aarch64 --version | head -1)"
