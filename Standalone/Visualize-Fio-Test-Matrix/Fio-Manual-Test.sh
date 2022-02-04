#!/usr/bin/env bash
set -eu; set -o pipefail
Say --Reset-Stopwatch

FIO_TOTAL=666
FIO_COUNT=0
FIO_SUCCESS=0
function Fio-Manual-Test() {
  local ver="$1"
  local caption="$2"
  local url="$3"
  FIO_COUNT=$((FIO_COUNT+1))
  local dirExtracted="/transient-builds/fio-extracted/$caption"
  # DOWNLOAD
  if [[ ! -d "$dirExtracted" ]]; then
      local dirBuffer="/transient-builds/fio-downloads"
      Say "${FIO_COUNT}/${FIO_TOTAL} Download: $caption"
      mkdir -p "$dirExtracted" "$dirBuffer"
      wget --no-check-certificate -O "$dirBuffer/${caption}.xz" "$url"
      if [[ "$ver" == "VER-3" ]]; then
        tar xJf "$dirBuffer/${caption}.xz" -C "$dirExtracted"
      else
        cat "$dirBuffer/${caption}.xz" | xz -d > "$dirExtracted/fio"
        chmod +x "$dirExtracted/fio"
      fi
  fi

  dirRun="$HOME/fio-runner"
  mkdir -p "$dirRun"
  rm -f "$dirRun/"*
  cp -f "$dirExtracted/"* "$dirRun/"

  # TEST, https://github.com/intel/fiovisualizer/issues/13 '--minimal', '--eta=0', '--status-interval=1'
  Say "${FIO_COUNT}/${FIO_TOTAL} Benchmark: [$caption]"
  for engine in libaio; do
      local err=0
      LD_LIBRARY_PATH="$dirRun" "$dirRun/fio" --name=test --randrepeat=1 --ioengine=$engine --gtod_reduce=1 --filename=$HOME/fio-test.tmp --bs=4k --size=32K --readwrite=read 2>&1 || err=$?
      if [[ "$err" -eq 0 ]]; then
        FIO_SUCCESS=$((FIO_SUCCESS+1))
        Say "${FIO_COUNT}/${FIO_TOTAL} (OK:$FIO_SUCCESS) $engine SUCCESS: [$caption]"
      else
        Say --Display-As=Error "${FIO_COUNT}/${FIO_TOTAL} (OK:$FIO_SUCCESS) $engine failed: [$caption]"
      fi
  done 
}

