#!/usr/bin/env bash
work=$HOME/Transient-Builds/KernelManagementLab; 
# work=/mnt/ftp-client/KernelManagementLab; 
mkdir -p "$(dirname $work)" 
cd $(dirname $work); 
rm -rf $work; 
git clone https://github.com/devizer/KernelManagementLab; 
cd KernelManagementLab/Universe.W3Top
time dotnet build -f netcoreapp2.2 -c Release -o bin/
cd bin/
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS="http://localhost:5010;https://0.0.0.0:5011"
dotnet Universe.W3Top.dll

function ignore() { 
git pull; 
nuget restore *.sln; 
if [ "$(command -v msbuild)" == "" ]; then cmd=xbuild; else cmd=msbuild; fi; echo Building using [$cmd]; \
eval time msbuild /t:Rebuild /p:Configuration=Release /v:m; \
cd MountLab/bin/Release; pdb2mdb *.exe *.dll; \
bash repack.sh; \
cd .; mono  MountLab.exe Monitor-V1

# pdb2mdb KernelManagementJam.dll; mono MountLab.exe Monitor-V1
# --profile=log:noalloc
}
