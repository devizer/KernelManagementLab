pushd /sys/devices >/dev/null
# sudo find . | grep -E 'name$' | xargs -d $'\n' sh -c 'for arg do echo "$arg: [$(sudo cat $arg)]"; done'
# sudo find . -type f | xargs -d $'\n' sh -c 'for arg do echo "$arg: [$(sudo cat $arg 2>/dev/null)]"  | grep ASUS; done'

popd >/dev/null

# root is not required
cat /sys/devices/virtual/dmi/id/board_{vendor,name,version}


echo '
./virtual/dmi/id/chassis_vendor: [ASUSTeK Computer Inc.        ]
./virtual/dmi/id/sys_vendor: [ASUSTeK Computer Inc.        ]
./virtual/dmi/id/board_vendor: [ASUSTeK Computer Inc.        ]

./virtual/dmi/id/product_name: [M50Vn               ]
./virtual/dmi/id/board_name: [M50Vn     ]
' >/dev/null

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
  echo "$f: [$(sudo cat $f 2>/dev/null)]"
done

echo '
/sys/devices/virtual/dmi/id/chassis_vendor: [Google]
/sys/devices/virtual/dmi/id/sys_vendor: [Google]
/sys/devices/virtual/dmi/id/board_vendor: [Google]
/sys/devices/virtual/dmi/id/product_name: [Google Compute Engine]
/sys/devices/virtual/dmi/id/board_name: [Google Compute Engine]

/sys/devices/virtual/dmi/id/chassis_vendor: [ASUSTeK Computer Inc.        ]
/sys/devices/virtual/dmi/id/sys_vendor: [ASUSTeK Computer Inc.        ]
/sys/devices/virtual/dmi/id/board_vendor: [ASUSTeK Computer Inc.        ]
/sys/devices/virtual/dmi/id/product_name: [M50Vn               ]
/sys/devices/virtual/dmi/id/board_name: [M50Vn     ]

/sys/devices/virtual/dmi/id/chassis_vendor: [Microsoft Corporation]
/sys/devices/virtual/dmi/id/sys_vendor: [Microsoft Corporation]
/sys/devices/virtual/dmi/id/board_vendor: [Microsoft Corporation]
/sys/devices/virtual/dmi/id/product_name: [Virtual Machine]
/sys/devices/virtual/dmi/id/board_name: [Virtual Machine]

ARMv8
/sys/devices/virtual/dmi/id/chassis_vendor: [Lenovo]
/sys/devices/virtual/dmi/id/sys_vendor: [Lenovo]
/sys/devices/virtual/dmi/id/board_vendor: [Lenovo]
/sys/devices/virtual/dmi/id/product_name: [HR330A            7X33CTO1WW    ]
/sys/devices/virtual/dmi/id/board_name: [FALCON     ]
/sys/firmware/devicetree/base/model: []


'
