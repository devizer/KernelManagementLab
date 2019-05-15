#!/usr/bin/env bash
# wget -q -nv --no-check-certificate -O - https://raw.githubusercontent.com/devizer/KernelManagementLab/master/build-w3-dashboard.sh | bash -s reinstall_service  
set -e
set -u

rm -rf KernelManagementLab
git clone https://github.com/devizer/KernelManagementLab
cd KernelManagementLab/Universe.W3Top
eval dotnet run $BENCH
