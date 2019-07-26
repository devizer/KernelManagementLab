#!/usr/bin/env bash
IFS=''
speedtest-cli --list | while read line; do
  escaped=$(echo $line | sed -r "s/'/\\\'/g")
  echo " { label: '"$escaped"' },"
done

