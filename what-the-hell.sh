mkdir "/with space"
mount -t tmpfs tmpfs "/with space"
mkdir /fuck\\off
mount -t tmpfs tmpfs /fuck\\off
mkdir "/fuck-��-������-off"
mount -t tmpfs tmpfs  "/fuck-��-������-off"
cat /proc/mounts
exit;


# output:
tmpfs /with\040space tmpfs rw,relatime 0 0
tmpfs /fuck\134off tmpfs rw,relatime 0 0
tmpfs /fuck-��-������-off tmpfs rw,relatime 0 0
tmpf /with\040space /fuck\134off /fuck-��-������-off

