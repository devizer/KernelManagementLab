work=$HOME/build/xnvme
git clone https://github.com/OpenMPDK/xNVMe.git $work
cd $work
time sudo bash ./xnvme/toolbox/pkgs/ubuntu-jammy.sh

meson setup builddir
cd builddir

# build xNVMe
meson compile

# install xNVMe
meson install

# uninstall xNVMe
# meson --internal uninstall

# add /usr/local/lib/x86_64-linux-gnu to /etc/ld.so.conf


