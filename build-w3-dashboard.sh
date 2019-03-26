#!/usr/bin/env bash
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
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS="http://localhost:5010;https://0.0.0.0:5011"
cd ClientApp; time yarn install; cd ..
time dotnet publish -c Release -o bin/ --self-contained -r linux-arm
cd bin
./Universe.W3Top
# time dotnet build -c Release; time dotnet run -c Release
