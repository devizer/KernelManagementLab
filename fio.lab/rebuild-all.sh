#!/usr/bin/env bash
script=https://raw.githubusercontent.com/devizer/test-and-build/master/install-build-tools-bundle.sh; (wget -q -nv --no-check-certificate -O - $script 2>/dev/null || curl -ksSL $script) | bash

export DEBIAN_FRONTEND=noninteractive
if [[ "$(command -v qemu-arm-static)" == "" || "$(command -v toilet)" == "" ]]; then 
  try-and-retry sudo apt-get update
  smart-apt-install -qq qemu-user-static toilet 
fi
try-and-retry docker pull multiarch/qemu-user-static:register
docker run --rm --privileged multiarch/qemu-user-static:register --reset

counter=0
errors=0;
mkdir -p result

# set -e
# nuget build will fail later in case of error

function set_title() {
  echo -e '\033k'$*'\033\\'
}

function build() {
  image=$1
  tag=$2
  public_name=$3
  err=0;

  # creating container, it will reused 4 times * all versions
  name="temp-builder-${tag}"
  echo ""
  Say "Container NAME: $name"
  docker rm -f $name >/dev/null 2>&1 || true
  cmd="docker pull ${image}:${tag} >/dev/null 2>&1"
  Say "Pull image [${image}:${tag}]"
  try-and-retry eval "$cmd"
  Say "Start container [$name]"
  docker run -d --privileged --name $name --rm "${image}:${tag}" bash -c "while true; do sleep 999; done"
  docker cp /usr/local/bin/File-IO-Benchmark "$name:/usr/local/bin/"
  ldd_version="$(docker exec -t $name ldd --version | head -1 |  awk '{print $NF}')"
  echo "$public_name: libc $ldd_version" >> result/versions.txt
  Say "Installing build tools for container [$name]"
  docker cp build-tools-in-container.sh "$name:/"
  docker exec -t $name bash /build-tools-in-container.sh
  Say "Container ready"

  # 1: libaio, 2: zlib
  cmd_i1="echo 'INSTALLING [libaio]'; test -d /etc/yum.repos.d && yum install libaio-devel -y; test -d /etc/apt && apt-get install -y libaio-dev"
  cmd_i2="echo 'INSTALLING [zlib]'; test -d /etc/yum.repos.d && yum install zlib-devel -y; test -d /etc/apt && apt-get install -y zlib1g-dev"
  cmd_r1="echo 'REMOVE [libaio]'; test -d /etc/yum.repos.d && yum remove libaio-devel -y; test -d /etc/apt && apt-get remove -y libaio-dev"
  cmd_r2="echo 'REMOVE [zlib]'; test -d /etc/yum.repos.d && yum remove zlib-devel -y; test -d /etc/apt && apt-get remove -y zlib1g-dev"
  
  # libaio & zlib vars
  options_commands=("${cmd_i1};${cmd_i2}" "${cmd_i1};${cmd_r2}" "${cmd_r1};${cmd_i2}" "${cmd_r1};${cmd_r2}")
  options_keys=("-libaio-zlib" "-libaio" "-zlib" "")
  
  # libaio varies only
  options_commands=("${cmd_i1};" "${cmd_r1};")
  options_keys=("-libaio" "")

  # options_commands=("${cmd_i1};")
  # options_keys=("-testlog")
  
  # cut next two lines
  # options_commands=("${cmd_i1};${cmd_i2}")
  # options_keys=("-skip")

  for (( i=0; i < ${#options_keys[@]}; i++ )); do 
  options_cmd="${options_commands[$i]}"; 
  options_key="${options_keys[$i]}"; 
  echo "[$options_key]: [$options_cmd]"
  cat fio-current.csv | while read url
  do
     counter=$((counter+1))
     # download fio-src
     fio_name=$(basename $url)
     vname="${fio_name%.*}"
     vname="${vname%.*}"
     set_title "$counter: $vname-$public_name"
     echo "($counter) Downloading [$vname] for [$vname-$public_name] from [$url]"
     try-and-retry curl -kSL -o fio_current.tar.gz "$url"
     mkdir -p fio-src
     rm -rf fio-src/*
     cd fio-src; 
     tar xzf ../fio_current.tar.gz; 
     cd ..
     Say "($counter) Clean up container for [$vname${options_key}-$public_name]"
     docker exec -t $name bash -c "rm -rf /build; rm -rf /out; rm -rf /usr/local/fio"
     Say "($counter) Configure options for [$options_key]"
     echo $options_cmd
     docker exec -t $name bash -c "$options_cmd"
     # building
     Say "($counter) Copy files to container for [$vname${options_key}-$public_name]"
     docker cp ./. "$name:/build/"
     Say "($counter) Exec BUILDING for [$vname${options_key}-$public_name]"
     mkdir -p result/$vname${options_key}-$public_name
     docker exec -t $name bash -c "cd /build; cd fio-src; bash ../in-container.sh" | tee result/$vname${options_key}-$public_name/build.log
     Say "($counter) Grab binaries from /out to [result/$vname${options_key}-$public_name]"
     docker cp "$name:/out/." result/$vname${options_key}-$public_name/
     mkdir -p result/_benchmarks
     cp result/$vname${options_key}-$public_name/Benchmark.log.gz result/_benchmarks/$vname${options_key}-$public_name.benchmark.log.gz 
     ls result/$vname${options_key}-$public_name/*.tar.gz >/dev/null 2>&1
     if [[ $? != 0 ]]; then
        mv result/$vname${options_key}-$public_name "result/$vname${options_key}-$public_name (not available)"
     fi
  done # versions
  done # options
  docker rm -f $name
}

build centos 7                                        amd64-rhel7
build centos 6                                        amd64-rhel6
build multiarch/ubuntu-debootstrap amd64-precise      amd64-precise
build multiarch/ubuntu-debootstrap amd64-focal        amd64-focal
build multiarch/ubuntu-debootstrap amd64-xenial       amd64-xenial

build multiarch/debian-debootstrap amd64-stretch      amd64-stretch
build multiarch/debian-debootstrap amd64-jessie       amd64-jessie
build multiarch/debian-debootstrap amd64-wheezy       amd64-wheezy

build multiarch/ubuntu-debootstrap amd64-trusty       amd64-trusty
build multiarch/ubuntu-debootstrap armhf-precise      armhf-precise

build multiarch/ubuntu-debootstrap armhf-xenial       armhf-xenial
build multiarch/ubuntu-debootstrap i386-precise       i386-precise
build multiarch/ubuntu-debootstrap i386-xenial        i386-xenial

build multiarch/ubuntu-debootstrap arm64-trusty       arm64-trusty
build multiarch/ubuntu-debootstrap arm64-xenial       arm64-xenial
build multiarch/ubuntu-debootstrap ppc64el-trusty     ppc64el-trusty
build multiarch/ubuntu-debootstrap ppc64el-xenial     ppc64el-xenial
build multiarch/debian-debootstrap powerpc-wheezy     powerpc-wheezy
build multiarch/debian-debootstrap armel-wheezy       armel-wheezy
build multiarch/debian-debootstrap armel-stretch      armel-stretch
build multiarch/debian-debootstrap mips64el-stretch   mips64el-stretch
build multiarch/debian-debootstrap mipsel-stretch     mipsel-stretch
build multiarch/debian-debootstrap mipsel-jessie      mipsel-jessie
build multiarch/debian-debootstrap mips-stretch       mips-stretch
build multiarch/debian-debootstrap mips-jessie        mips-jessie

build multiarch/ubuntu-debootstrap amd64-focal        amd64-focal
build multiarch/debian-debootstrap amd64-buster       amd64-buster
build multiarch/debian-debootstrap amd64-bullseye     amd64-bullseye
build multiarch/ubuntu-debootstrap amd64-bionic       amd64-bionic

exit; 
