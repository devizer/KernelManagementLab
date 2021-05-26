#!/usr/bin/env bash
script=https://raw.githubusercontent.com/devizer/test-and-build/master/install-build-tools-bundle.sh; (wget -q -nv --no-check-certificate -O - $script 2>/dev/null || curl -ksSL $script) | bash

export DEBIAN_FRONTEND=noninteractive
if [[ "$(command -v qemu-arm-static)" == "" || "$(command -v toilet)" == "" ]]; then 
  try-and-retry sudo apt-get update -q
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

libaio_versions=""
function prepare_libaio_src() {
  cat libaio-ver-links.txt | while read -r ver link; do
      if [[ -z "$ver" ]]; then continue; fi
      Say "Downloading $ver: $link"
      work=/transient-builds/libaio-src/$ver
      mkdir -p $work
      rm -rf $work/*
      pushd $work
      wget -O $ver $link
      tar xzf $ver
      rm -f $ver
      cd lib*
      Say "VER $ver"
      libaio_versions="$libaio_versions $ver"
      # time make prefix=/transient-builds/libaio-dev/$ver install
      popd
  done
}
prepare_libaio_src
libaio_versions="0.3.112 0.3.111 0.3.110 0.3.108 0.3.107 0.3.106"
Say "libaio versions: [$libaio_versions]"
# exit

function build() {
  image=$1
  tag=$2
  public_name=$3
  prepare_script=$4
  err=0;

  # creating container, it will reused 4 times * all versions
  name="fio-builder-${public_name}"
  echo ""
  Say "Container NAME: $name"
  docker rm -f $name >/dev/null 2>&1 || true
  cmd="docker pull ${image}:${tag} >/dev/null 2>&1"
  Say "Pull image [${image}:${tag}]"
  try-and-retry eval "$cmd"
  Say "Start container [$name]"
  docker run -d --privileged --hostname $name --name $name --rm "${image}:${tag}" bash -c "while true; do sleep 999; done"
  docker exec -t $name bash -c "echo $image:$tag > /tmp/image-id"
  for script in File-IO-Benchmark Say try-and-retry; do
    docker cp /usr/local/bin/$script "$name:/usr/local/bin/"
  done
  ldd_version="$(docker exec -t $name ldd --version | head -1 |  awk '{print $NF}')"
  ldd_version="${ldd_version//[$'\t\r\n']}"
  Say "Installing build tools for container [$name]: $prepare_script"
  docker cp build-tools-in-container.sh "$name:/"
  docker cp /transient-builds/libaio-src/ "$name:/transient-builds/libaio-src/"
  libaio_version_cmd="bash -c \"apt-cache policy libaio-dev | grep andidate | awk '{print \\\$NF}'\""
  docker exec -t $name bash -c "source /build-tools-in-container.sh; $prepare_script"
  # yum info libaio-devel | grep Version | head -1
  libaio_version="$(docker exec -t $name apt-cache policy libaio-dev | grep andidate | awk '{print $NF}')" 
  echo "$public_name: libc $ldd_version, libaio: $libaio_version" | tee -a result/versions.txt
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
  options_keys=("-libaio-system" "-libaio-missing")
  
  options_commands=();
  options_keys=();
  
  for ver in $libaio_versions; do
    options_commands+=("echo =========== COMPILING LIBAIO $ver =========; cat /build/in-constaner-libaio.sh; bash /build/in-constaner-libaio.sh $ver;")
    options_keys+=("-libaio-$ver") 
  done

  # ---=== ONLY libaio ===---
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
     Say "($counter) Copy files to container for [$vname${options_key}-$public_name]"
     docker cp ./. "$name:/build/"
     Say "($counter) Configure options for [$options_key]"
     echo $options_cmd
     docker exec -t $name bash -c "$options_cmd"
     # building
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
  docker rmi -f "${image}:${tag}"
}

build multiarch/ubuntu-debootstrap amd64-focal        amd64-focal         prepare_debian
build multiarch/ubuntu-debootstrap amd64-precise      amd64-precise       prepare_debian
exit;
build ubuntu hirsute                                  amd64-hirsute       prepare_debian
build multiarch/ubuntu-debootstrap amd64-trusty       amd64-trusty        prepare_debian
build ubuntu groovy                                   amd64-groovy        prepare_debian
build centos 8                                        amd64-rhel8         prepare_centos
build quay.io/centos/centos stream                    amd64-centosstream  prepare_centos_stream
build centos 7                                        amd64-rhel7         prepare_centos
build centos 6                                        amd64-rhel6         prepare_centos


build multiarch/ubuntu-debootstrap amd64-xenial       amd64-xenial        prepare_debian

build multiarch/debian-debootstrap amd64-stretch      amd64-stretch       prepare_debian
build multiarch/debian-debootstrap amd64-jessie       amd64-jessie        prepare_debian
build multiarch/debian-debootstrap amd64-wheezy       amd64-wheezy        prepare_debian

build multiarch/ubuntu-debootstrap armhf-precise      armhf-precise       prepare_debian
build multiarch/ubuntu-debootstrap armhf-xenial       armhf-xenial        prepare_debian
build multiarch/ubuntu-debootstrap armhf-bionic       armhf-bionic        prepare_debian
build multiarch/ubuntu-debootstrap armhf-focal        armhf-focal         prepare_debian

build multiarch/ubuntu-debootstrap i386-precise       i386-precise        prepare_debian
build multiarch/ubuntu-debootstrap i386-xenial        i386-xenial         prepare_debian
build multiarch/ubuntu-debootstrap i386-bionic        i386-bionic         prepare_debian

build multiarch/ubuntu-debootstrap arm64-trusty       arm64-trusty        prepare_debian
build multiarch/ubuntu-debootstrap arm64-xenial       arm64-xenial        prepare_debian
build multiarch/ubuntu-debootstrap arm64-bionic       arm64-bionic        prepare_debian
build multiarch/ubuntu-debootstrap arm64-focal        arm64-focal         prepare_debian

build multiarch/ubuntu-debootstrap ppc64el-trusty     ppc64el-trusty      prepare_debian
build multiarch/ubuntu-debootstrap ppc64el-xenial     ppc64el-xenial      prepare_debian
build multiarch/ubuntu-debootstrap ppc64el-bionic     ppc64el-bionic      prepare_debian
build multiarch/ubuntu-debootstrap ppc64el-focal      ppc64el-focal      prepare_debian

build multiarch/debian-debootstrap powerpc-wheezy     powerpc-wheezy      prepare_debian
build multiarch/ubuntu-debootstrap powerpc-yakkety    powerpc-yakkety     prepare_debian

build multiarch/debian-debootstrap armel-wheezy       armel-wheezy        prepare_debian
build multiarch/debian-debootstrap armel-stretch      armel-stretch       prepare_debian
build multiarch/debian-debootstrap armel-buster       armel-buster        prepare_debian

build multiarch/debian-debootstrap mips64el-stretch   mips64el-stretch    prepare_debian
build multiarch/debian-debootstrap mipsel-stretch     mipsel-stretch      prepare_debian
build multiarch/debian-debootstrap mipsel-jessie      mipsel-jessie       prepare_debian
build multiarch/debian-debootstrap mips-stretch       mips-stretch        prepare_debian
build multiarch/debian-debootstrap mips-jessie        mips-jessie         prepare_debian

build multiarch/ubuntu-debootstrap amd64-focal        amd64-focal         prepare_debian
build multiarch/debian-debootstrap amd64-buster       amd64-buster        prepare_debian
build multiarch/debian-debootstrap amd64-bullseye     amd64-bullseye      prepare_debian
build multiarch/ubuntu-debootstrap amd64-bionic       amd64-bionic        prepare_debian

exit; 
