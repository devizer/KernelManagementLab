#!/usr/bin/env bash
# wget -q -nv --no-check-certificate -O - https://raw.githubusercontent.com/devizer/KernelManagementLab/master/build-w3-dashboard.sh | bash -s reinstall_service  
set -e
set -u

THIS="$(pwd)"

export DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
function header() {
  if [[ $(uname -s) != Darwin ]]; then
    startAt=${startAt:-$(date +%s)}; elapsed=$(date +%s); elapsed=$((elapsed-startAt)); elapsed=$(TZ=UTC date -d "@${elapsed}" "+%_H:%M:%S");
  fi
  LightGreen='\033[1;32m'; Yellow='\033[1;33m'; RED='\033[0;31m'; NC='\033[0m'; LightGray='\033[1;2m';
  printf "${LightGray}${elapsed:-}${NC} ${LightGreen}$1${NC} ${Yellow}$2${NC}\n"; 
}
counter=0;
function say() { counter=$((counter+1)); header "STEP $counter" "$1"; }

TRANSIENT_ROOT=$HOME/build/transient
if [[ -d "/transient-builds" ]]; then TRANSIENT_ROOT=/transient-builds; fi
if [[ -d "/ssd" ]]; then TRANSIENT_ROOT=/ssd/transient-builds; fi

pushd build >/dev/null
./inject-git-info.ps1
popd >/dev/null

