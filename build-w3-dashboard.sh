#!/usr/bin/env bash
work=$HOME/Transient-Builds/KernelManagementLab; 
# work=/mnt/ftp-client/KernelManagementLab; 
mkdir -p "$(dirname $work)" 
cd $(dirname $work); 
rm -rf $work; 
git clone https://github.com/devizer/KernelManagementLab; 
cd KernelManagementLab/Universe.W3Top
# time dotnet publish -c Release -o bin/arm/ --self-contained -r linux-arm
time dotnet publish -c Release -o bin/
cd bin/
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS="http://localhost:5010;https://0.0.0.0:5011"
time dotnet Universe.W3Top.dll
