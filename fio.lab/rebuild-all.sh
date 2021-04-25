#!/usr/bin/env bash
script=https://raw.githubusercontent.com/devizer/test-and-build/master/install-build-tools-bundle.sh; (wget -q -nv --no-check-certificate -O - $script 2>/dev/null || curl -ksSL $script) | bash

export DEBIAN_FRONTEND=noninteractive
if [[ "$(command -v qemu-arm-static)" == "" || "$(command -v toilet)" == "" ]]; then 
  try-and-retry sudo apt-get update
  smart-apt-install -qq qemu-user-static toilet 
fi
try-and-retry docker pull multiarch/qemu-user-static:register
docker run --rm --privileged multiarch/qemu-user-static:register --reset

rm -f runtimes/missed.log
counter=0
errors=0;

# set -e
# nuget build will fail later in case of error

function build() {
  image=$1
  tag=$2
  public_name=$3
  counter=$((counter+1))
  err=0;

  cat fio-current.csv | while read url
  do
     fio_name=$(basename $url)
     echo "Downloading [$url]"
     try-and-retry curl -kSL -o fio_current.tar.gz "$url"
     mkdir -p fio-src
     rm -rf fio-src/*
     cd fio-src; 
     tar xzf ../fio_current.tar.gz; 
     cd ..
     name="temp-builder-${tag}"
     echo ""
     Say "NAME: $name, [$fio_name]"
     docker rm -f $name >/dev/null 2>&1 || true
     cmd="docker pull ${image}:${tag} >/dev/null 2>&1"
     Say "Pull image [${image}:${tag}]"
     try-and-retry eval "$cmd"
     Say "Start container [$name]"
     docker run -d --name $name --rm "${image}:${tag}" bash -c "while true; do sleep 999; done"
     Say "Copy files to container"
     docker cp ./. "$name:/build/"
     Say "Exec BUILDING"
     docker exec -t $name bash -c "cd /build; ls -la; bash in-container.sh"
  done

}

build multiarch/ubuntu-debootstrap amd64-precise      amd64
build centos 6 linux-rhel.6                           rhel6
build multiarch/ubuntu-debootstrap armhf-precise      armhf
build multiarch/ubuntu-debootstrap i386-precise       i386
build multiarch/ubuntu-debootstrap arm64-trusty       arm64
build multiarch/ubuntu-debootstrap ppc64el-trusty     ppc64el
build multiarch/debian-debootstrap powerpc-wheezy     powerpc
build multiarch/debian-debootstrap armel-wheezy       armel
build multiarch/debian-debootstrap mips64el-stretch   mips64el

exit; 
