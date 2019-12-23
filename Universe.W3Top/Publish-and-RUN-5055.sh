#!/usr/bin/env bash
# wget -q -nv --no-check-certificate -O - https://raw.githubusercontent.com/devizer/KernelManagementLab/master/build-w3-dashboard.sh | bash -s reinstall_service  
set -e
set -u
if [[ $(uname -m) == armv7* ]]; then rid=linux-arm; elif [[ $(uname -m) == aarch64 ]]; then rid=linux-arm64; elif [[ $(uname -m) == x86_64 ]]; then rid=linux-x64; fi; if [[ $(uname -s) == Darwin ]]; then rid=osx-x64; fi;
echo "The current OS architecture: $rid"

unset MSBuildSDKsPath
dir=$(pwd)

pushd ../build >/dev/null
./inject-git-info.ps1
popd >/dev/null

function run_prod() {
  cd $dir
  export ASPNETCORE_ENVIRONMENT=Production
  cd ClientApp; time (yarn install); cd ..
  time dotnet publish -c Debug -f netcoreapp2.2 /p:DefineConstants="DEBUG" -o bin/local  --self-contained -r $rid
  cd bin/local
  echo VERSION: $(dotnet ./Universe.W3Top.dll --version)
  dotnet ./Universe.W3Top.dll | tee log.log
}


export DUMPS_ARE_ENABLED=On
export ASPNETCORE_URLS="http://0.0.0.0:5055;https://0.0.0.0:5056"
run_prod
