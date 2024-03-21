Say --Reset-Stopwatch
Say "Clean up build folder [on host]"
cd /mnt/c/_GIT/KernelManagementLab
work=$HOME/build/KernelManagementJam-Mono-Tests
rm -rf "$work"/*
mkdir -p $work
Say "Copy $(pwd) --> $work [on host]"
cp -a * $work
cd $work
Reset-Target-Framework -fw net462 -l latest
# for prj in KernelManagementJam KernelManagementJam.Tests Universe.FioStream Universe.FioStream.Binaries; do
for prj in KernelManagementJam.Tests; do
  cd $prj
  Say "Restore: $prj"
  time try-and-retry nuget restore -Verbosity quiet *.csproj
  msbuild /t:Restore /v:q
  cd ..
done
cd KernelManagementJam.Tests
msbuild /t:Build /v:q /p:Configuration=Release
dir="."; url=https://raw.githubusercontent.com/devizer/glist/master/bin/libMono.Unix.so/download-libMono-Unix-so.sh; (wget -q -nv --no-check-certificate -O - $url 2>/dev/null || curl -sSL $url) | bash -s "$dir"
dir="."; url=https://raw.githubusercontent.com/devizer/glist/master/bin/libNativeLinuxInterop/download-libNativeLinuxInterop-so.sh; (wget -q -nv --no-check-certificate -O - $url 2>/dev/null || curl -sSL $url) | bash -s "$dir"
cd bin/Release/net462

Say "Test Locally"
nunit3-console --workers=1 --where 'test =~ /HugeCrossInfo_Tests/' KernelManagementJam.Tests.dll
echo "Complete: Local Test"


mkdir -p ~/.ssh; printf "Host *\n   StrictHostKeyChecking no\n   UserKnownHostsFile=/dev/null" > ~/.ssh/config
# remotely packages: rsync, mono, nunit3-console (installer also needs sudo)
for phone in MIX2 TIGER; do
  eval user='$'${phone}_USER
  eval pass='$'${phone}_PASSWORD
  eval ip='$'${phone}_IP
  Say "TEST ON PHONE $phone ($ip)"
  remote_folder="/data/data/com.termux/files/home/"cpu-remote-tests
  # sshpass -p $pass ssh -p 8022 $user@$ip bash -c "echo CREATING REMOTE FOLDER: [${remote_folder}]; mkdir -p ${remote_folder}"
  # sshpass -p $pass ssh -p 8022 $user@$ip mkdir -p "${remote_folder}"
  sshpass -p $pass rsync -rvz -e 'ssh -p 8022' ./. $user@$ip:"${remote_folder}" >/dev/null
  sshpass -p $pass ssh -p 8022 $user@$ip bash -c "echo; cd ${remote_folder}; nunit3-console --workers=1 --where 'test =~ /HugeCrossInfo_Tests/' KernelManagementJam.Tests.dll"
  echo "Complete: TEST ON PHONE $phone"
done
