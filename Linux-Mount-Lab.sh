work=$HOME/KernelManagementLab; \
# work=/mnt/ftp-client/KernelManagementLab; \
cd $(dirname $work); \
rm -rf $work; \
git clone https://github.com/devizer/KernelManagementLab; \
cd KernelManagementLab; \
git pull; \
nuget restore *.sln; \
if [ "$(command -v msbuild)" == "" ]; then cmd=xbuild; else cmd=msbuild; fi; echo Building using [$cmd]; \
eval time $cmd /t:Rebuild /p:Configuration=Release /v:m; \
cd MountLab/bin/Release; \
bash repack.sh; \
cd ..; mono --profile=log:noalloc MountLab.exe Monitor-V1

# pdb2mdb KernelManagementJam.dll; mono MountLab.exe Monitor-V1
