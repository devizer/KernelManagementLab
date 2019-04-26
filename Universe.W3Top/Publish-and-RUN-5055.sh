#!/usr/bin/env bash
# wget -q -nv --no-check-certificate -O - https://raw.githubusercontent.com/devizer/KernelManagementLab/master/build-w3-dashboard.sh | bash -s reinstall_service  
set -e
set -u
if [[ $(uname -m) == armv7* ]]; then rid=linux-arm; elif [[ $(uname -m) == aarch64 ]]; then rid=linux-arm64; elif [[ $(uname -m) == x86_64 ]]; then rid=linux-x64; fi; if [[ $(uname -s) == Darwin ]]; then rid=osx-x64; fi;
echo "The current OS architecture: $rid"

dir=$(pwd)

pushd ../build >/dev/null
./inject-git-info.ps1
popd >/dev/null

function run_prod() {
  cd $dir
  export ASPNETCORE_ENVIRONMENT=Production
  cd ClientApp; time (yarn install); cd ..
  time dotnet publish -c Release /p:DefineConstants="DUMPS" -o bin/local
  cd bin/local
  dotnet ./Universe.W3Top.dll
}

function reinstall_service() {
  cd $dir
  export ASPNETCORE_ENVIRONMENT=Production
  cd ClientApp; time (yarn install); cd ..
  # time dotnet publish -c Release /p:DefineConstants="DUMPS" -o bin/service
  time dotnet publish -c Release /p:DefineConstants="DUMPS" -o bin/service --self-contained -r $rid
  cd bin/service
  if [[ -z "${INSTALL_DIR-}" ]]; then INSTALL_DIR=/opt/w3top; fi
  sudo mkdir -p $INSTALL_DIR
  sudo rm -rf $INSTALL_DIR/*
  sudo cp -fR * $INSTALL_DIR
  sudo chmod -x $INSTALL_DIR/*.dll
  bash $INSTALL_DIR/install-systemd-service.sh
}

function deploy_to_gae()
{
  reinstall_service
  cd $INSTALL_DIR
  cd ..
  time sudo bash -c 'tar cf - w3top | pv | gzip -9 > w3top.tar.gz'; ls -la w3top.tar.gz
  gsutil cp w3top.tar.gz gs://pet-projects-europe/
}

export DUMPS_ARE_ENABLED=On
export ASPNETCORE_URLS="http://0.0.0.0:5055;https://0.0.0.0:5056"
run_prod
