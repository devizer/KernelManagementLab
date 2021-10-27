#!/usr/bin/env bash
base_images="ubuntu:focal ubuntu:bionic ubuntu:21.10 debian:stretch-slim debian:buster-slim debian:bullseye-slim"
for base_image in $base_images; do
  tag=$(echo "$base_image" | sed -r 's/:/-/g')
  echo $tag, $base_image 

  time docker build \
    --build-arg BASE_IMAGE="${base_image}" \
    --build-arg BUILD_URL="${BUILD_URL}" \
    --build-arg JOB_URL="${JOB_URL}" \
    --build-arg BUILD_SOURCEVERSION="${BUILD_SOURCEVERSION}" \
    --build-arg BUILD_SOURCEBRANCHNAME="${BUILD_SOURCEBRANCHNAME}" \
    --build-arg BUILD_BUILDID="${BUILD_BUILDID}" \
    -t w3top-tmp/$tag . #| tee Log/x64-build-image-log.log

done
