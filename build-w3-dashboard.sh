#!/usr/bin/env bash
# wget -q -nv --no-check-certificate -O - https://raw.githubusercontent.com/devizer/KernelManagementLab/master/build-w3-dashboard.sh | bash -s reinstall_service  
set -e
set -u
if [[ $(uname -m) == armv7* ]]; then rid=linux-arm; elif [[ $(uname -m) == aarch64 ]]; then rid=linux-arm64; elif [[ $(uname -m) == x86_64 ]]; then rid=linux-x64; fi; if [[ $(uname -s) == Darwin ]]; then rid=osx-x64; fi;
echo "The current OS architecture: $rid"

work=$HOME/build/w3top-source
if [[ -d "/transient-builds" ]]; then work=/transient-builds/w3top-source; fi
if [[ -d "/ssd" ]]; then work=/ssd/transient-builds/w3top-source; fi
mkdir -p "$(dirname $work)"
cd $(dirname $work);
rm -rf KernelManagementLab;
git clone https://github.com/devizer/KernelManagementLab;
cd KernelManagementLab
# dotnet restore --disable-parallel
cd Universe.W3Top
dir=$(pwd)

pushd ../build >/dev/null
./inject-git-info.ps1
popd >/dev/null

function run_debug() {
  cd $dir
  export ASPNETCORE_ENVIRONMENT=Development
  rm -rf ClientApp/build 2>/dev/null
  cd ClientApp; time (yarn install); cd ..
  dotnet run -c Debug -f netcoreapp3.1
}

function run_prod() {
  cd $dir
  export ASPNETCORE_ENVIRONMENT=Production
  cd ClientApp; time (yarn install); cd ..
  dotnet restore --disable-parallel
  time dotnet publish -c Release -f netcoreapp3.1 /p:DefineConstants="DUMPS" -o bin/ --self-contained -r $rid
  cd bin
  ./Universe.W3Top
}

function reinstall_service() {
  cd $dir
  export ASPNETCORE_ENVIRONMENT=Production
  cd ClientApp; 
    time yarn install;
    time yarn test;
    time yarn build 
  cd ..
  # time dotnet publish -c Release -f netcoreapp2.2 /p:DefineConstants="DUMPS" -o bin/service
  dotnet restore || dotnet restore --disable-parallel || true
  echo -e "\nBUILDING..........."
  time SKIP_CLIENTAPP=true dotnet publish -c Release -f netcoreapp3.1 /p:DefineConstants="DUMPS" -o bin/service --self-contained -r $rid
  cd bin/service
  chmod 644 *.dll
  chmod 755 Universe.W3Top
  chmod 755 install-systemd-service.sh
  if [[ -z "${INSTALL_DIR:-}" ]]; then INSTALL_DIR=/opt/w3top; fi
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
  time sudo bash -c 'tar cf - w3top | pv | pigz -9 > w3top.tar.gz'; ls -la w3top.tar.gz
  gsutil cp w3top.tar.gz gs://pet-projects-europe/
}

export DUMPS_ARE_ENABLED=Off
export ASPNETCORE_URLS="http://0.0.0.0:5010;https://0.0.0.0:5011"
# run_prod
# reinstall_service
# cmd="$1"
cmd="${1:-reinstall_service}"
eval $cmd
echo "*** D O N E ***"
