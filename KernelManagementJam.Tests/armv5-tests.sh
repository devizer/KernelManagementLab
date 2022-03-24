work=$HOME/build/KernelManagementJam-Mono-Tests
mkdir -p $work
cd $work
test ! -d KernelManagementLab && git clone https://github.com/devizer/KernelManagementLab
cd KernelManagementLab
git reset --hard
git clean -xfd
git pull
Reset-Target-Framework -fw net462 -l latest
for prj in KernelManagementJam KernelManagementJam.Tests Universe.FioStream Universe.FioStream.Binaries; do
  cd $prj
  echo NUGET RESTORE PROJECT: *.csproj
  time nuget restore *.csproj
  cd ..
done
cd KernelManagementJam.Tests
msbuild /t:Build /p:Configuration=Release
cd bin/Release/net462
nunit3-console --workers=1 KernelManagementJam.Tests.dll
