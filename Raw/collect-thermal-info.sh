report=/opt/tmp/thermal-from-sys.tar
mkdir -p $(dirname $report)
tar -cf $report -T /dev/null

# sensors
for basedir in /sys/class/thermal /sys/class/hwmon; do
        tar uf $report $(echo $basedir/*/*)
done

# PC and Motherboard name
files='
/sys/devices/virtual/dmi/id/chassis_vendor
/sys/devices/virtual/dmi/id/sys_vendor
/sys/devices/virtual/dmi/id/board_vendor
/sys/devices/virtual/dmi/id/product_name
/sys/devices/virtual/dmi/id/board_name
/sys/firmware/devicetree/base/model
/proc/device-tree/model
'
for f in $files; do
  if [[ -s $f ]]; then
        tar uf $report $f
  fi
done

# Names of PCI Devices
lspci > /tmp/lspci || true; tar uf $report /tmp/lspci;

# Name of the CPU
cp /proc/cpuinfo /tmp/cpuinfo; tar uf $report /tmp/cpuinfo;
gzip -f $report

# https://paste.c-net.org выдаст короткую ссылку на выгруженный thermal-from-sys.tar.gz
curl -kSL --upload-file ${report}.gz 'https://paste.c-net.org/'


