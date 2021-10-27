#!/usr/bin/env bash
base_images="ubuntu:focal ubuntu:bionic ubuntu:21.10 debian:stretch-slim debian:buster-slim debian:bullseye-slim"
for base_image in $base_images; do
  tag_short=$(echo "$base_image" | sed -r 's/:/-/g')
  tag=w3top-tmp/$tag_short
  echo $tag, $base_image
   

  time docker build \
    --build-arg BASE_IMAGE="${base_image}" \
    --build-arg BUILD_URL="${BUILD_URL}" \
    --build-arg JOB_URL="${JOB_URL}" \
    --build-arg BUILD_SOURCEVERSION="${BUILD_SOURCEVERSION}" \
    --build-arg BUILD_SOURCEBRANCHNAME="${BUILD_SOURCEBRANCHNAME}" \
    --build-arg BUILD_BUILDID="${BUILD_BUILDID}" \
    -t $tag . #| tee Log/x64-build-image-log.log
    
    docker run --rm -it $tag bash -c "script=https://raw.githubusercontent.com/devizer/test-and-build/master/install-build-tools-bundle.sh; (wget -q -nv --no-check-certificate -O - $script 2>/dev/null || curl -ksSL $script) | bash; list-packages" > packages-$tag_short.log

done

first='
w3top-tmp/debian-bullseye-slim    265MB
w3top-tmp/debian-buster-slim      257MB
w3top-tmp/debian-stretch-slim     247MB
w3top-tmp/ubuntu-21.10            275MB
w3top-tmp/ubuntu-bionic           276MB
w3top-tmp/ubuntu-focal            282MB
'

next='
w3top-tmp/debian-bullseye-slim   211MB
w3top-tmp/debian-buster-slim     204MB
w3top-tmp/debian-stretch-slim    197MB
w3top-tmp/ubuntu-21.10           207MB
w3top-tmp/ubuntu-bionic          200MB
w3top-tmp/ubuntu-focal           212MB
'