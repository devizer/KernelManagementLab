#!/usr/bin/env bash
# This script propogates builtin private libssl v1.1.1 if shared libssl v1.1.* is absent
set -e
set -u
pushd `dirname $0` > /dev/null; ScriptPath=`pwd`; popd > /dev/null

if [[ ! -f "$ScriptPath/Universe.W3Top" ]]; then echo ERROR: publish the project first; exit 1; fi

export HTTP_PORT="${HTTP_PORT:-5050}"
export HTTP_HOST="${HTTP_HOST:-0.0.0.0}"
export RESPONSE_COMPRESSION="${RESPONSE_COMPRESSION:-True}"
export DUMPS_ARE_ENABLED="${DUMPS_ARE_ENABLED:-False}"

export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
export BLOCK_DEVICE_VISIBILITY_THRESHOLD=2048
export FORCE_HTTPS_REDIRECT=False

export PID_FILE_FULL_PATH=/var/run/w3top.pid
export ASPNETCORE_URLS="http://$HTTP_HOST:$HTTP_PORT"
export INSTALL_DIR="$ScriptPath"

if [[ -n "$(command -v ldconfig)" ]] && [[ -z "$(ldconfig -p | grep libssl.so.1.1)" ]]; then
  export LD_LIBRARY_PATH="${LD_LIBRARY_PATH:-}:$ScriptPath/optional/libssl-1.1"
else
  echo "Using preinstalled libssl.so.1.1"
fi

pushd "$ScriptPath" >/dev/null
./Universe.W3Top
popd >/dev/null
