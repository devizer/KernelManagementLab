function build_fio() {
  mode="$1"
  opt=""; suffix="dynamic"; if [[ $mode == static ]]; then opt="--build-static"; suffix="static"; fi
  destdir="${basedir}/publish-$suffix-$v"
  make clean
  ./configure --help 2>&1 > "${basedir}/confligure-$v-help.txt" 2>&1
  ./configure --prefix="${destdir}" $opt 2>&1 | tee "${basedir}/confligure-$suffix-$v.log"
  # make -j
  make -j V=1 install
  strip "${destdir}/bin/fio"
  mkdir -p "${basedir}/fio-cdn"
  cp -f "${destdir}/bin/fio" "${basedir}/fio-cdn/fio-$v-x64-$suffix"
}

basedir=/Volumes/Temp/fio
basedir=$HOME/build
for vm in $(seq 36 -1 17); do
  v="3.$vm"
  srcdir="${basedir}/src-$v"
  mkdir -p "${srcdir}"
  curl -ksfSL -o "${srcdir}/_$v.tar.gz" "https://brick.kernel.dk/snaps/fio-$v.tar.gz"
  tar xzf "${srcdir}/_$v.tar.gz" -C "${srcdir}"
  cd "${srcdir}"
  cd fio*
  build_fio static
  build_fio dynamic
  # break;
done
