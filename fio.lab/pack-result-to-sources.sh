tmp=/tmp/convert-fio
mkdir -p sources
for d in fio*/; do
  echo $d
  mkdir -p $tmp
  rm -rf $tmp/*
  if [[ -s "$d/fio-stripped.tar.gz" ]]; then
    echo "$d/fio-stripped.tar.gz"
    tar xzf $d/fio-stripped.tar.gz -C $tmp
    gzip -9 $tmp/fio
    mv $tmp/fio.gz sources/${d::-1}.gz
  fi
done
