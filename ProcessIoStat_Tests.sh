#!/usr/bin/env bash
pushd KernelManagementJam.Tests
sudo dotnet test  -c Release -f netcoreapp2.2 --filter "FullyQualifiedName~ProcessIoStat_Tests" | tee io-log.tmp
popd>/dev/null
