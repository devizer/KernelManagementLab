#!/usr/bin/env bash
bins=$(find -type d | grep -E '/(bin|obj)$' | grep -vE '/node_modules/')
for d in $bins; do echo "Removing $d"; rm -rf "$d"; done

