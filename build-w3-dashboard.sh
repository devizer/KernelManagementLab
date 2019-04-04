#!/usr/bin/env bash
if [[ $(uname -m) == armv7* ]]; then rid=linux-arm; elif [[ $(uname -m) == aarch64 ]]; then rid=linux-arm64; elif [[ $(uname -m) == x86_64 ]]; then rid=linux-x64; fi; if [[ $(uname -s) == Darwin ]]; then rid=osx-x64; fi;
echo "The current OS architecture: $rid" 

work=$HOME
if [[ -d "/ssd" ]]; then work=/ssd; fi
work=$work/Transient-Builds/KernelManagementLab; 
# work=/mnt/ftp-client/KernelManagementLab; 
mkdir -p "$(dirname $work)" 
cd $(dirname $work); 
rm -rf $work; 
git clone https://github.com/devizer/KernelManagementLab; 
cd KernelManagementLab/Universe.W3Top
dir=$(pwd)

function run_debug() {
cd $dir
export ASPNETCORE_ENVIRONMENT=Development
rm -rf ClientApp/build 2>/dev/null
cd ClientApp; time (yarn install); cd ..
dotnet run -c Debug
}

function run_prod() {
cd $dir
export ASPNETCORE_ENVIRONMENT=Production
cd ClientApp; time (yarn install); cd ..
time dotnet publish -c Release /p:DefineConstants="DUMPS" -o bin/ --self-contained -r $rid
cd bin
./Universe.W3Top
}

function reinstall_serive() {
cd $dir
export ASPNETCORE_ENVIRONMENT=Production
cd ClientApp; time (yarn install); cd ..
time dotnet publish -c Release /p:DefineConstants="DUMPS" -o bin/service
cd bin/service
if [[ -z $INSTALL_DIR ]]; then INSTALL_DIR=/opt/w3top; fi
sudo mkdir -p $INSTALL_DIR
sudo rm -rf $INSTALL_DIR/*
sudo cp -fR * $INSTALL_DIR
}

export DUMPS_Are_Enabled=Off
export ASPNETCORE_URLS="http://0.0.0.0:5010;https://0.0.0.0:5011"
# run_prod
reinstall_serive
