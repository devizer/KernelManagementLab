mkdir -p DUMPS
cur=$(pwd)
find -type d -name dumps | while read d; do
  echo $d
  pushd $d
    cp -r * $cur/DUMPS
  popd
done

