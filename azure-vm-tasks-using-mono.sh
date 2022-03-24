#!/usr/bin/env bash
script=https://raw.githubusercontent.com/devizer/test-and-build/master/install-build-tools-bundle.sh; (wget -q -nv --no-check-certificate -O - $script 2>/dev/null || curl -ksSL $script) | bash >/dev/null
Say --Reset-Stopwatch

export VM_SSH_PORT=2207 VM_MEM=3000M 
export VM_CPUS=${VM_CPUS:-2}
Say "VM: [$VM_KEY], CPUs:[$VM_CPUS], MEM [$VM_MEM]"

mkdir -p ~/.ssh 
echo '
Host *
   StrictHostKeyChecking no
   UserKnownHostsFile /dev/null
   LogLevel ERROR
' > ~/.ssh/config

home=~
export VM_STORAGE="${VM_STORAGE:-$home/vm}"
export VM_USER="${VM_USER:-root}"
export VM_PASS="${VM_PASS:-pass}"
export VM_SSH_PORT="${VM_SSH_PORT:-2202}"
export VM_MEM=2000M

api_code_url=https://raw.githubusercontent.com/devizer/glist/master/vm-build-agent.sh
api_code_file=/tmp/vm-build-agent-$(whoami).sh
try-and-retry wget -q -nv --no-check-certificate -O "$api_code_file" "$api_code_url" 2>/dev/null || try-and-retry curl -ksSL -o "$api_code_file" "$api_code_url"
source "$api_code_file"


DownloadVM $VM_KEY

export VM_SSH_PORT=2207 VM_MEM=3000M 
export VM_CPUS=${VM_CPUS:-2}

RunVM $VM_KEY
if [ "$VM_SSHFS_MAP_ERROR" -ne 0 ]; then
  Say "ERROR. Unable to map guest fs. Code is $VM_SSHFS_MAP_ERROR"
  exit 234
fi


cmd='
echo;
free -m;
sudo ip addr show;
echo starting in $(pwd); 
lazy-apt-update

# remove the two lines below
Say "jq [$(jq --version)]"
Say "Get-GitHub-Latest-Release: [$(command -v Get-GitHub-Latest-Release)]"
url=https://raw.githubusercontent.com/devizer/glist/master/Install-Latest-Docker-Compose.sh; (wget -q -nv --no-check-certificate -O - $url 2>/dev/null || curl -ksSL $url) | bash
url=https://raw.githubusercontent.com/devizer/glist/master/Install-Latest-PowerShell.sh; (wget -q -nv --no-check-certificate -O - $url 2>/dev/null || curl -ksSL $url) | bash

cd ~; git clone https://github.com/devizer/KernelManagementLab; pwd; uname -a
cd KernelManagementLab


export VSTEST_CONNECTION_TIMEOUT=300000
export SHORT_FIO_TESTS=True
export DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=1
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1


Say "env"
printenv | sort

apt-get update; apt-get install build-essential binutils -y -q | { grep "Setting\|Unpacking" || true; }
script=https://raw.githubusercontent.com/devizer/test-and-build/master/install-build-tools-bundle.sh; (wget -q -nv --no-check-certificate -O - $script 2>/dev/null || curl -ksSL $script) | bash
export INSTALL_DIR=/usr/local TOOLS="bash git jq 7z nano gnu-tools cmake curl mono"; time (script="https://master.dl.sourceforge.net/project/gcc-precompiled/build-tools/Install-Build-Tools.sh?viasf=1"; (wget -q -nv --no-check-certificate -O - $script 2>/dev/null || curl -ksSL $script) | bash)
# Say "AOTing $INSTALL_DIR/lib/mono/4.5/mscorlib.dll"
# time mono --aot -O=all "$INSTALL_DIR/lib/mono/4.5/mscorlib.dll"
script="https://master.dl.sourceforge.net/project/gcc-precompiled/ca-certificates/update-ca-certificates.sh?viasf=1"; (wget -q -nv --no-check-certificate -O - $script 2>/dev/null || curl -ksSL $script) | bash
rm -f /usr/local/bin/grep
export MSBUILD_INSTALL_VER=16.10.1
export MSBUILD_INSTALL_VER=16.6
export MSBUILD_INSTALL_DIR=/usr/local; script="https://master.dl.sourceforge.net/project/gcc-precompiled/msbuild/Install-MSBuild.sh?viasf=1"; (wget -q -nv --no-check-certificate -O - $script 2>/dev/null || curl -ksSL $script) | bash
url=https://raw.githubusercontent.com/devizer/glist/master/bin/net-test-runners.sh; (wget -q -nv --no-check-certificate -O - $url 2>/dev/null || curl -sSL $url) | bash
curl -kSL -o $HOME/test-cpu-usage.sh https://raw.githubusercontent.com/devizer/Universe.CpuUsage/master/test-on-mono-only-platforms.sh
time bash -e $HOME/test-cpu-usage.sh
err=$?; Say "Write exit code [$err] to $(pwd)/tests-exit-code"
echo $err | tee tests-exit-code
Say "DONE. Complete"
'

Say "VM_ROOT_FS is [$VM_ROOT_FS]"
EvaluateCommand "$cmd"

Say "Grab Test Result"
cp -f -r $VM_ROOT_FS/root/KernelManagementLab/* .

testExitCode="$(cat tests-exit-code)"
Say "tests-exit-code: $testExitCode"
# ls -la $VM_ROOT_FS/root/KernelManagementLab || true
# ShutdownVM $VM_KEY
if [[ "$testExitCode" != "0" ]]; then
  exit 222;
fi

