work=$HOME/Transient-Builds/KernelManagementLab; \
# work=/mnt/ftp-client/KernelManagementLab; \
mkdir -p "$(dirname $work)" \
cd $(dirname $work); \
rm -rf $work; \
git clone https://github.com/devizer/KernelManagementLab; \
cd KernelManagementLab; \
git pull; \
nuget restore *.sln; \
if [ "$(command -v msbuild)" == "" ]; then cmd=xbuild; else cmd=msbuild; fi; echo Building using [$cmd]; \
eval time msbuild /t:Rebuild /p:Configuration=Release /v:m; \
cd MountLab/bin/Release; pdb2mdb *.exe *.dll; \
bash repack.sh; \
cd .; mono  MountLab.exe Monitor-V1

# pdb2mdb KernelManagementJam.dll; mono MountLab.exe Monitor-V1
# --profile=log:noalloc
