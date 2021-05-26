#!/usr/bin/env bash
set -e
ver=$1
echo "BUILDING LIBAIO $ver"
cd /transient-builds/libaio-src/$ver/lib*
rm -rf /transient-builds/libaio-dev || true
# time make prefix=/transient-builds/libaio-dev install
time make prefix=/usr/local install
echo '
# export CFLAGS="-O2 -I/transient-builds/libaio-dev/include/"
# export LDFLAGS="-L/transient-builds/libaio-dev/lib/"
# export LD_LIBRARY_PATH=/transient-builds/libaio-dev/lib
export FIO_CONFIGURE_OPTIONS="--build-static"
' > /vars.sh
