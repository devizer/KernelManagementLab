#!/usr/bin/env bash
dotnet test --filter "FullyQualifiedName~Hwmon" -f netcoreapp3.1
