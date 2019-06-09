#!/usr/bin/env bash
# wget -q -nv --no-check-certificate -O - https://raw.githubusercontent.com/devizer/KernelManagementLab/master/build-w3-dashboard.sh | bash -s reinstall_service  
set -e
set -u

function header() { LightGreen='\033[1;32m';Yellow='\033[1;33m';RED='\033[0;31m'; NC='\033[0m'; printf "${LightGreen}$1${NC} ${Yellow}$2${NC}\n"; }
counter=0;
function say() { counter=$((counter+1)); header "STEP $counter" "$1"; }


work=$HOME/transient-builds
if [[ -d "/transient-builds" ]]; then work=/transient-builds; fi
if [[ -d "/ssd" ]]; then work=/ssd/transient-builds; fi

clone=$work/publish/w3top-bin
say "Clean w3top-bin clone location: [$clone]"
rm -rf $clone; mkdir -p $(dirname $clone)
say "Loading working copy"
git clone git@github.com:devizer/w3top-bin $clone

work=$work/publish/KernelManagementLab;
say "Loading source to [$work]"
# work=/mnt/ftp-client/KernelManagementLab;
mkdir -p "$(dirname $work)"
cd $(dirname $work);
rm -rf $work;
git clone https://github.com/devizer/KernelManagementLab;
cd KernelManagementLab/Universe.W3Top
dir=$(pwd)

pushd ../build >/dev/null
./inject-git-info.ps1
popd >/dev/null

verFile=../build/AppGitInfo.json
ver=$(cat $verFile | jq -r ".Version")
cp $verFile $clone/public/version.json

say "yarn install [$ver]"
cd ClientApp; time (yarn install); cd ..

for r in linux-musl-x64 rhel.6-x64 linux-x64 linux-arm linux-arm64; do

  say "Building $r [$ver]"
  time dotnet publish -c Release /p:DefineConstants="DUMPS" -o bin/$r --self-contained -r $r
  pushd bin/$r
  chmod 644 *.dll
  chmod 755 Universe.W3Top
  chmod 755 install-systemd-service.sh

  say "Compressing $r [$ver] as GZIP"
  time sudo bash -c "tar cf - . | pv | gzip -9 > ../w3top-$r.tar.gz"
  cp ../w3top-$r.tar.gz $clone/public/
  # say "Compressing $r [$ver] as XZ"
  # time sudo bash -c "tar cf - w3top | pv | xz -1 -z > ../w3top-$r.tar.xz"
  # say "Compressing $r [$ver] as 7z"
  # 7z a "../w3top-$r.7z" -m0=lzma -mx=1 -mfb=256 -md=256m -ms=on

  popd
done

if [ -n "${SKIP_GIT_PUSH:-}" ]; then exit; fi


pushd $clone >/dev/null
git add --all .
say "Commit binaries [$ver]"
git commit -am "Update $ver"
say "Publish binaries [$ver]"
git push
popd >/dev/null

say "Collecting garbage"
bash $clone/git-gc/defrag.sh

say "DONE: [$ver]"

