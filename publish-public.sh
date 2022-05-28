#!/usr/bin/env bash
# wget -q -nv --no-check-certificate -O - https://raw.githubusercontent.com/devizer/KernelManagementLab/master/build-w3-dashboard.sh | bash -s reinstall_service  
set -e
set -u

Say --Reset-Stopwatch || true
 
export DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0

function header() {
  if [[ $(uname -s) != Darwin ]]; then
    startAt=${startAt:-$(date +%s)}; elapsed=$(date +%s); elapsed=$((elapsed-startAt)); elapsed=$(TZ=UTC date -d "@${elapsed}" "+%_H:%M:%S");
  fi
  LightGreen='\033[1;32m'; Yellow='\033[1;33m'; RED='\033[0;31m'; NC='\033[0m'; LightGray='\033[1;2m';
  printf "${LightGray}${elapsed:-}${NC} ${LightGreen}$1${NC} ${Yellow}$2${NC}\n"; 
}
counter=0;
function say() { counter=$((counter+1)); header "STEP $counter" "$1"; }
say reset>/dev/null

DOTNETHOME=/transient-builds/dotnet4publish
function _install_proper_sdk_() {
  for sdk in 3.1 3.1.120; do
    export DOTNET_VERSIONS=$sdk DOTNET_TARGET_DIR=$DOTNETHOME/$sdk SKIP_DOTNET_ENVIRONMENT=true
    script=https://raw.githubusercontent.com/devizer/test-and-build/master/lab/install-DOTNET.sh; (wget -q -nv --no-check-certificate -O - $script 2>/dev/null || curl -ksSL $script) | bash;
  done
}

_install_proper_sdk_

work=$HOME/transient-builds
if [[ -d "/transient-builds" ]]; then work=/transient-builds; fi
# if [[ -d "/ssd" ]]; then work=/ssd/transient-builds; fi

clone=$work/publish/w3top-bin
say "Clean w3top-bin clone location: [$clone]"
rm -rf $clone; mkdir -p $(dirname $clone)
say "Loading w3top-bin working copy"
if [ -n "${SKIP_GIT_PUSH:-}" ]; then w3topBinRepo=https://github.com/devizer/w3top-bin; else w3topBinRepo=git@github.com:devizer/w3top-bin; fi
git clone ${w3topBinRepo} $clone

work=$work/publish/KernelManagementLab;
say "Loading source to [$work]"
# work=/mnt/ftp-client/KernelManagementLab;
mkdir -p "$(dirname $work)"
cd $(dirname $work);
rm -rf $work;
if [[ -n "${TRAVIS:-}" ]]; then
  git clone https://github.com/devizer/KernelManagementLab
else
  git clone git@github.com:devizer/KernelManagementLab
fi
cd KernelManagementLab
# upgrade-2-to-3
root=$(pwd)
# repo root
cd Universe.W3Top
dir=$(pwd)

pushd ../build >/dev/null
./inject-git-info.ps1
popd >/dev/null

verFile=../build/AppGitInfo.json
ver=$(cat $verFile | jq -r ".Version")
cp $verFile $clone/public/version.json
echo $ver > $clone/public/version.txt

say "yarn install [$ver]"
cd ClientApp; time (yarn install); cd ..

say "yarn test [$ver]"
cd ClientApp; time (yarn test); cd ..

say "yarn build [$ver]"
cd ClientApp; time (yarn build); cd ..

# export MSBuildSDKsPath=/usr/share/dotnet/sdk/3.1.408/Sdks
for r in linux-musl-x64 rhel.6-x64 linux-x64 linux-arm linux-arm64; do

  if [[ $r == rhel.6-x64 ]]; then sdk=3.1.120; else sdk=3.1; fi
  say "Building $r [$ver] using [sdk $sdk]"
  dotnetpath=$DOTNETHOME/$sdk
  export PATH="$dotnetpath:$PATH"
  unset MSBuildSDKsPath || true
  time SKIP_CLIENTAPP=true $dotnetpath/dotnet publish -c Release -f netcoreapp3.1 /p:DefineConstants="DUMPS" -o bin/$r --self-contained -r $r
  pushd bin/$r
  # rm -f System.*.a - included in manifest
  chmod 644 *.dll
  chmod 755 Universe.W3Top
  chmod 755 install-systemd-service.sh

  say "Compressing $r [$ver] as GZIP"
  echo $ver > VERSION
  compress="pigz -p 8 -b 128"
  time sudo bash -c "tar cf - . | pv | $compress -9 > ../w3top-$r.tar.gz"
  [ "${TRAVIS:-}" == "true" ] && sha256sum ../w3top-$r.tar.gz | awk '{print $1}' > ../w3top-$r.tar.gz.hash256
  sha256sum ../w3top-$r.tar.gz | awk '{print $1}' > ../w3top-$r.tar.gz.sha256
  cp ../w3top-$r.tar.gz* $clone/public/
  say "Compressing $r [$ver] as XZ"
  time sudo bash -c "tar cf - . | pv | xz -9 -e -z > ../w3top-$r.tar.xz"
  sha256sum ../w3top-$r.tar.xz | awk '{print $1}' > ../w3top-$r.tar.xz.sha256
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

cd $root
say "RUN Create-GitHub-Release.sh [$ver]"
bash Create-GitHub-Release.sh

say "DONE: [$ver]"
