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
  time try-and-retry nuget restore *.csproj
  cd ..
done
cd KernelManagementJam.Tests
msbuild /t:Build /p:Configuration=Release
dir="."; url=https://raw.githubusercontent.com/devizer/glist/master/bin/libMono.Unix.so/download-libMono-Unix-so.sh; (wget -q -nv --no-check-certificate -O - $url 2>/dev/null || curl -sSL $url) | bash -s "$dir"
cd bin/Release/net462
nunit3-console --workers=1 KernelManagementJam.Tests.dll
