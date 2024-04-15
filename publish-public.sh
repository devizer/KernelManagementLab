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
  for sdk in 3.1 3.1.120 6.0 8.0; do
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

export PUBLISH_GIT_TAG="v${ver}"
export PUBLISH_GIT_REPO=w3top-bin
export PUBLISH_GIT_OWNER=devizer
export PUBLISH_GIT_PATH=$clone/publish/w3top-bin
export PUBLISH_GIT_TEXT="Installation and upgrade options: <br> https://github.com/devizer/w3top-bin#reinstallation-of-precompiled-binaries. <br> <br> History: <br> https://github.com/devizer/KernelManagementLab/blob/master/WHATSNEW.md"
export PUBLISH_GIT_FILES="?"

say "GitHub Release Parameters"
printenv | grep PUBLISH_GIT | sort

say "yarn install [$ver]"
cd ClientApp; time (yarn install); cd ..

say "yarn test [$ver]"
cd ClientApp; time (yarn test); cd ..

say "yarn build [$ver]"
cd ClientApp; time (yarn build); cd ..

# export MSBuildSDKsPath=/usr/share/dotnet/sdk/3.1.408/Sdks
function build_self_contained() {
  suffix="$1"
  r="$2"
  sdk="$3"
  fw="$4"

  say "Building w3top-$suffix.tar RID=$r [$ver] using [sdk=$sdk] using target [fw=$fw]"
  dotnetpath=$DOTNETHOME/$sdk
  export PATH="$dotnetpath:$PATH"
  unset MSBuildSDKsPath || true
  Reset-Target-Framework -fw "$fw"
  time SKIP_CLIENTAPP=true $dotnetpath/dotnet publish -c Release -f $fw /p:DefineConstants="DUMPS" -o bin/$r --self-contained -r $r -v q
  # openssl 1.1
  if [[ -f $root/Dependencies/libssl-1.1-$r.tar.xz ]]; then
    mkdir -p bin/$r/optional/libssl-1.1
    tar xJf $root/Dependencies/libssl-1.1-$r.tar.xz -C bin/$r/optional/libssl-1.1
  fi
  pushd bin/$r
  # rm -f System.*.a - included in manifest
  chmod 644 *.dll
  chmod 755 Universe.W3Top
  chmod 755 Universe.W3Top.sh
  chmod 755 install-systemd-service.sh

  say "Compressing $r [$ver] as GZIP"
  echo $ver > VERSION
  compress="pigz -p 8 -b 128 -9" # v1
  compress="gzip -9" # v2
  compress="7z a -mx=9 -tgzip -si -so -bso0 -bsp0 -mmt=1 1.gz" #vBest
  time sudo bash -c "tar cf - . | pv | $compress > ../w3top-$suffix.tar.gz"
  sha256sum ../w3top-$r.tar.gz | awk '{print $1}' > ../w3top-$suffix.tar.gz.sha256
  cp ../w3top-$suffix.tar.gz* $clone/public/
  say "Compressing $r [$ver] as XZ"
  time sudo bash -c "tar cf - . | pv | xz -9 -e -z > ../w3top-$suffix.tar.xz"
  sha256sum ../w3top-$r.tar.xz | awk '{print $1}' > ../w3top-$suffix.tar.xz.sha256
  cp ../w3top-$suffix.tar.xz* $clone/public/
  popd
}

build_self_contained linux-x64                 linux-x64 8.0 net8.0
build_self_contained linux-x64-for-legacy-os   linux-x64 6.0 net6.0
build_self_contained linux-arm                 linux-arm 8.0 net8.0
build_self_contained linux-arm-for-legacy-os   linux-arm 6.0 net6.0
build_self_contained linux-arm64               linux-arm64 8.0 net8.0
build_self_contained linux-arm64-for-legacy-os linux-arm64 6.0 net6.0
build_self_contained linux-musl-x64            linux-musl-x64 8.0 net8.0
build_self_contained rhel.6-x64                rhel.6-x64 3.1.120 netcoreapp3.1

sf_release_dir=/transient-builds/w3top-new-version-for-sf
say "Prepare sf release: [${sf_release_dir}]"
mkdir -p "${sf_release_dir}"; rm -rf "${sf_release_dir}"/*
mkdir -p "${sf_release_dir}/$ver"
cp -fr $clone/public/* "${sf_release_dir}/$ver"
tree -sh "${sf_release_dir}" || true

if [ -n "${SKIP_GIT_PUSH:-}" ]; then exit; fi

pushd $clone >/dev/null
git add --all .
say "Commit binaries [$ver]"
git commit -am "Update $ver ***NO_CI*** (by deploy pipeline)"
say "Publish binaries [$ver]"
git push
say "Tag w3top-bin [$ver]"
git tag -f "v${ver}"
say "Tag w3top-bin [$ver]"
git push --tags

# PUBLISH RELEASE TO W3Top-bin repo
body="Installation and upgrade options: <br> https://github.com/devizer/w3top-bin#reinstallation-of-precompiled-binaries. <br> <br> History: <br> https://github.com/devizer/KernelManagementLab/blob/master/WHATSNEW.md"
echo "KEY: ${#GITHUB_RELEASE_TOKEN} chars"
echo "clone: [$clone]"
echo "work: [$work]"
Say "About [$work/WHATSNEW.md]"
ls -la "$work/WHATSNEW.md" || true
Say "About [$clone/public]"
ls -la "$clone/public" || true
dpl --provider=releases --api-key=$GITHUB_RELEASE_TOKEN \
  --file-glob=true --overwrite=true \
  --name="W3Top Stable ${ver}" \
  --body="$PUBLISH_GIT_TEXT" \
  --file="$clone/public/w3top*.tar.*" --file="$work/WHATSNEW.md" \
  --skip-cleanup \
  --repo=devizer/w3top-bin

popd >/dev/null

say "Collecting garbage and trigger pipeline for w3top-bin"
export TRIGGER_COMMIT_MESSAGE="Update $ver"
bash $clone/git-gc/defrag.sh

cd $root
say "RUN Create-GitHub-Release.sh [$ver]"
echo "Current Folder is [$(pwd)]"
bash Create-GitHub-Release.sh

say "DONE: [$ver]"
