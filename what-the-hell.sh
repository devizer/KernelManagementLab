mkdir "/with space"
mount -t tmpfs tmpfs "/with space"
mkdir /fuck\\off
mount -t tmpfs tmpfs /fuck\\off
mkdir "/fuck-по-русски-off"
mount -t tmpfs tmpfs  "/fuck-по-русски-off"
cat /proc/mounts
exit;


# output:
tmpfs /with\040space tmpfs rw,relatime 0 0
tmpfs /fuck\134off tmpfs rw,relatime 0 0
tmpfs /fuck-по-русски-off tmpfs rw,relatime 0 0
tmpf /with\040space /fuck\134off /fuck-по-русски-off

