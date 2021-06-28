#!/usr/bin/env bash
function sepia() {
  f=$1
  # -set colorspace RGB   -sepia-tone 50%
  convert $f.jpg -sepia-tone 80% -quality 35% $f-sepia.jpg
  mv $f-sepia.jpg $f.jpg
}
sepia background-wallpper-1950
sepia background-wallpper-2560
sepia background-wallpper-3840
sepia background-wallpper-5120

