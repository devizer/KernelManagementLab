files='
/sys/devices/virtual/dmi/id/chassis_vendor
/sys/devices/virtual/dmi/id/sys_vendor
/sys/devices/virtual/dmi/id/board_vendor

/sys/devices/virtual/dmi/id/product_name
/sys/devices/virtual/dmi/id/board_name

/sys/firmware/devicetree/base/model
'
for f in $files; do
  echo "$f: [$(cat $f 2>/dev/null)]"
done
