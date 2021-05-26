#!/usr/bin/env bash
ver=$1
echo "BUILDING LIBAIO $ver"
cd /transient-builds/libaio-src/$ver/lib*
rm -rf /transient-builds/libaio-dev || true
time make prefix=/transient-builds/libaio-dev install
echo '
CFLAGS="-O2 -I/transient-builds/libaio-dev/include/"
LDFLAGS="-L/transient-builds/libaio-dev/lib/"
' > /vars.sh
