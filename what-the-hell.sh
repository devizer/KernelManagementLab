mkdir -p "/media/with space"
mount -t tmpfs tmpfs "/media/with space"
mkdir -p /media/fuck\\off
mount -t tmpfs tmpfs /media/fuck\\off
mkdir -p "/media/fuck-��-������-off"
mount -t tmpfs tmpfs  "/media/fuck-��-������-off"
cat /proc/mounts
exit;


# output:
tmpfs /with\040space tmpfs rw,relatime 0 0
tmpfs /fuck\134off tmpfs rw,relatime 0 0
tmpfs /fuck-��-������-off tmpfs rw,relatime 0 0
tmpf /with\040space /fuck\134off /fuck-��-������-off

