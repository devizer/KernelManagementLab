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
# time dotnet publish -c Release -o bin/arm/ --self-contained -r linux-arm
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS="http://localhost:5010;https://0.0.0.0:5011"
cd ClientApp; time (yarn install && yarn build); cd ..
dotnet run -c Debug

function ignore() {
time dotnet publish -c Release /p:DefineConstants="TRACE" -o bin/ --self-contained -r $rid
cd bin
./Universe.W3Top
# time dotnet build -c Release; time dotnet run -c Release
}