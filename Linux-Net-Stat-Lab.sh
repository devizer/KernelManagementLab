#!/usr/bin/env bash
work=$HOME/Transient-Builds/KernelManagementLab; 
# work=/mnt/ftp-client/KernelManagementLab; 
mkdir -p "$(dirname $work)" 
cd $(dirname $work); 
rm -rf $work; 
git clone https://github.com/devizer/KernelManagementLab; 
cd KernelManagementLab/LinuxNextStatLab
dotnet run



function ignore() {

time dotnet build -f netcoreapp2.2 -c Release
cd bin/Release/netcoreapp2.2
time dotnet MountLab.dll


work=$HOME/KernelManagementLab; \
cd $HOME; \
rm -rf $work; \
git clone https://github.com/devizer/KernelManagementLab; \
cd KernelManagementLab; \
 
git pull; \
nuget restore *.sln; \
xbuild /t:Rebuild /p:Configuration=Debug /v:m; \
cd LinuxNetStatLab/bin/Debug; \
pdb2mdb LinuxNetStatLab.exe; mono LinuxNetStatLab.exe


}
