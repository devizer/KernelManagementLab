#!/usr/bin/env bash
script=https://raw.githubusercontent.com/devizer/test-and-build/master/install-build-tools-bundle.sh; (wget -q -nv --no-check-certificate -O - $script 2>/dev/null || curl -ksSL $script) | bash >/dev/null
Say --Reset-Stopwatch

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

export VM_SSH_PORT=2207 VM_MEM=2000M VM_CPUS=2

RunVM $VM_KEY


cmd='
echo;
free -m;
sudo ip addr show;
echo starting in $(pwd); 
lazy-apt-update
cd ~; git clone https://github.com/devizer/KernelManagementLab; pwd; uname -a
cd KernelManagementLab

Say "Install NET Core 6.0 & 3.1"
export DOTNET_VERSIONS="3.1 6.0" DOTNET_TARGET_DIR=/usr/share/dotnet
script=https://raw.githubusercontent.com/devizer/test-and-build/master/lab/install-DOTNET.sh; 
(wget -q -nv --no-check-certificate -O - $script 2>/dev/null || curl -ksSL $script) | bash; 
test -s /usr/share/dotnet/dotnet && sudo ln -f -s /usr/share/dotnet/dotnet /usr/local/bin/dotnet

export VSTEST_CONNECTION_TIMEOUT=300000
export SHORT_FIO_TESTS=True
export DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=1
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

Say "Installing actual CA Bundle for Buster $(uname -m)"
file=/usr/local/share/ssl/cacert.pem
url=https://curl.haxx.se/ca/cacert.pem
sudo mkdir -p $(dirname $file)
sudo wget -q -nv --no-check-certificate -O $file $url 2>/dev/null || sudo curl -ksSL $url -o $url
test -s $file && export CURL_CA_BUNDLE="$file"

Say "env"
printenv | sort

Say "dotnet test -f netcoreapp3.1 -c Release --logger trx"
time dotnet test -f netcoreapp3.1 -c Release --logger trx
e=$?
echo $e > tests-exit-code
Say "TEST STATUS: $e"
'

Say "VM_ROOT_FS is [$VM_ROOT_FS]"
EvaluateCommand "$cmd"

Say "Grab Test Result"
cp -f -r $VM_ROOT_FS/root/KernelManagementLab/* .

testExitCode="$(cat tests-exit-code)"
Say "tests-exit-code: $testExitCode"
ls -la $VM_ROOT_FS/root/KernelManagementLab/*
# ShutdownVM $VM_KEY
if [[ "$testExitCode" != "0" ]]; then
  exit 222;
fi

