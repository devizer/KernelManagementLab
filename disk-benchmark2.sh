#!/usr/bin/env bash
set -e

rm -rf KernelManagementLab || true
git clone https://github.com/devizer/KernelManagementLab
cd KernelManagementLab/BenchmarkLab
dotnet build -c Release -o bin/temp
cd bin/temp
if [ -z "$BENCH" ]; then BENCH="-s=102400 -f=42 -b=4096 -t=30000"; fi
dotnet Universe.Benchmark.dll -p="/linux live" -s=1024000 -b=4096 -t=30000
