exclude='
/proc/pagetypeinfo
/proc/sysrq-trigger
/proc/vmallocinfo
/proc/sched_debug
'

target=$HOME/pseudo-root
mkdir -p $HOME/pseudo-root
for dir in "/sys/devices" "/sys/block" "/proc" ; do
  find $dir | while read file; do
    is_a_prcess_folder="$(echo $file | grep -E '^/proc/[123456789]')"
    is_excluded="$(echo $exclude | grep $file)"
    if [[ -z "$is_a_prcess_folder" && $is_excluded == "" && -f $file ]]; then
      echo "--> $file"
      fileSize=$(stat -c"%s" "$file" 2>/dev/null || stat --printf="%s" "$file")
      if [[ "$fileSize" -lt 1000000 ]]; then
        mkdir -p "${target}$(dirname $file)"
        timeout -s9 3 cp $file ${target}${file} || rm -f ${target}${file}

        # large?
        fileSize2=$(stat -c"%s" "${target}${file}" 2>/dev/null || stat --printf="%s" "${target}${file}")
        if [[ "$fileSize2" -gt 500000 ]]; then
          rm -f "${target}${file}"
        fi

      fi
    fi
  done
done
