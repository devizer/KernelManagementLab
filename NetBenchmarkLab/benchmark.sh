#!/usr/bin/env bash
set -e

export DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

cd ~
rm -rf net || true
git clone https://github.com/devizer/KernelManagementLab net
cd net/NetBenchmarkLab
dotnet build -c Release -o bin/temp
cd bin/temp
dotnet NetBenchmarkLab.dll
