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
  # export ASPNETCORE_ENVIRONMENT=Production
  cd ClientApp; time (yarn install); cd ..
  rm -rf bin/local
  rm -rf ~/.cache/google-chrome/
  # /p:PublishTrimmed=true /p:PublishReadyToRun=true
  time dotnet publish -c Release -f netcoreapp3.1 /p:PublishReadyToRun=true /p:DefineConstants="NO_DEBUG" -o bin/local --self-contained -r $rid
  function dot_restore() {
    Say "HACK: Restoring"
    cmdRestore="dotnet restore >/dev/null || true; msbuild /t:restore >/dev/null || true"
    eval "$cmdRestore" 
    pushd ..; eval "$cmdRestore"; popd
    Say "HACK: Restored"
    sleep 2
  }
  dot_restore
  cd bin/local
  echo VERSION: $(dotnet ./Universe.W3Top.dll --version); sleep 3
  echo "DIR is [$(pwd)]"
  find .
  nohup bash -c "sleep 2; xdg-open http://localhost:5055" &
  dotnet ./Universe.W3Top.dll | tee log.log
  # sudo bash -c "dotnet ./Universe.W3Top.dll" | tee log.log
}


export DUMPS_ARE_ENABLED=SideBySide
export ASPNETCORE_URLS="http://0.0.0.0:5055;https://0.0.0.0:5056"
run_prod
