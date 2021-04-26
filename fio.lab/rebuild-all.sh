#!/usr/bin/env bash
script=https://raw.githubusercontent.com/devizer/test-and-build/master/install-build-tools-bundle.sh; (wget -q -nv --no-check-certificate -O - $script 2>/dev/null || curl -ksSL $script) | bash

export DEBIAN_FRONTEND=noninteractive
if [[ "$(command -v qemu-arm-static)" == "" || "$(command -v toilet)" == "" ]]; then 
  try-and-retry sudo apt-get update
  smart-apt-install -qq qemu-user-static toilet 
fi
try-and-retry docker pull multiarch/qemu-user-static:register
docker run --rm --privileged multiarch/qemu-user-static:register --reset

if [[ -n "$TF_BUILD" ]]; then
  rm -rf result/* || true
fi
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
     vname="${fio_name%.*}"
     vname="${name%.*}"
     echo "Downloading [$vname] from [$url]"
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
     mkdir -p result/$fio_name-$public_name
     docker exec -t $name bash -c "cd /build; ls -la; cd fio-src; bash ../in-container.sh" | tee result/$fio_name/$public_name/build.log
     Say "Grab binaries from /out to [result/$fio_name-$public_name]"
     docker cp "$name:/out/." result/$fio_name-$public_name/
     docker rm -f $name
  done

}

build multiarch/ubuntu-debootstrap amd64-xenial       amd64-xenial

build multiarch/debian-debootstrap amd64-stretch      amd64-stretch
build multiarch/debian-debootstrap amd64-jessie       amd64-jessie
build multiarch/debian-debootstrap amd64-wheezy       amd64-wheezy
build multiarch/debian-debootstrap amd64-wheezy       amd64-stretch

build multiarch/ubuntu-debootstrap amd64-trusty       amd64-trusty
build multiarch/ubuntu-debootstrap amd64-precise      amd64-precise
build centos 6                                        amd64-rhel6
build multiarch/ubuntu-debootstrap armhf-precise      armhf-precise
build multiarch/ubuntu-debootstrap armhf-xenial       armhf-xenial

build multiarch/ubuntu-debootstrap i386-precise       i386-precise
build multiarch/ubuntu-debootstrap i386-xenial        i386-xenial
build multiarch/ubuntu-debootstrap arm64-trusty       arm64-trusty
build multiarch/ubuntu-debootstrap arm64-xenial       arm64-xenial
build multiarch/ubuntu-debootstrap ppc64el-trusty     ppc64el-trusty
build multiarch/ubuntu-debootstrap ppc64el-xenial     ppc64el-xenial
build multiarch/debian-debootstrap powerpc-wheezy     powerpc-wheezy
build multiarch/debian-debootstrap powerpc-sid        powerpc-sid
build multiarch/debian-debootstrap armel-wheezy       armel-wheezy
build multiarch/debian-debootstrap armel-stretch      armel-stretch
build multiarch/debian-debootstrap mips64el-stretch   mips64el-stretch

exit; 
