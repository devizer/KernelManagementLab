#!/usr/bin/env bash
set -e

rm -rf KernelManagementLab || true
git clone https://github.com/devizer/KernelManagementLab
cd KernelManagementLab/BenchmarkLab
eval dotnet run $BENCH
