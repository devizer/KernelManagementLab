artifact_url="https://devizer.visualstudio.com/1bf34956-fcb5-495a-ab40-de38808a9407/_apis/build/builds/2057/artifacts?artifactName=fio&api-version=6.0&%24format=zip"
artifact_url=https://sourceforge.net/projects/fio/files/build-matrix/fio-matrix3.7z/download
work=/transient-builds/fio-build-matrix
mkdir -p $work
cd $work
try-and-retry wget -O fio-matrix3.7z "$artifact_url"
7z -y x fio-matrix3.7z
cd fio-matrix3
tmp=/tmp/convert-fio
mkdir -p sources
for d in fio*/; do
  echo $d
  file=${d::-1}
  mkdir -p $tmp
  rm -rf $tmp/*
  if [[ -s "$d/fio-stripped.tar.gz" ]]; then
    echo "   < $d/fio-stripped.tar.gz"
    tar xzf $d/fio-stripped.tar.gz -C $tmp
    echo "   > sources/${file}"
    cp $tmp/fio sources/${file}
    echo "   > sources/${file}.gz"
    gzip -9 < $tmp/fio > sources/${file}.gz
    echo "   > sources/${file}.xz"
    xz -9 -e < $tmp/fio > sources/${file}.xz
    echo "   > sources/${file}.bz2"
    bzip2 -z -9 < $tmp/fio > sources/${file}.bz2
  fi
done
