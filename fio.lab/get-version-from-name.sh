cat fio-current.csv | while read url; do
  fio_name=$(basename $url)
  name="${fio_name%.*}"
  name="${name%.*}"
  echo "$name: $url"
done
