for arch in i386 armel armhf arm64 amd64 powerpc mips64el ppc64el; do
  rm -rf /tmp/file-list
  ls -1 *$arch* | grep -v -E "\.bz2" | grep -v -E "\.gz" | grep -v -E "\.xz" > /tmp/file-list
  mkdir -p by-architecture
    echo "$arch --> gz"
    tar -cf - -T /tmp/file-list | gzip -9 > by-architecture/fio-$arch.tar.gz
    echo "$arch --> bz2"
    tar -cf - -T /tmp/file-list | bzip2 -z -9 > by-architecture/fio-$arch.tar.bz2
    echo "$arch --> xz"
    tar -cf - -T /tmp/file-list | xz -z -e -9 > by-architecture/fio-$arch.tar.xz
  echo ""
done
