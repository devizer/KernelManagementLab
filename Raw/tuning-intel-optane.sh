# https://www.flashmemorysummit.com/Proceedings2019/08-07-Wednesday/20190807_SOFT-202-1_Verma.pdf

DEVS="nvme0n1 "
for dev in $DEVS; do
  echo "Prep /dev/$dev"
  SYSFS=/sys/block/$dev/queue
  # echo 0 > $SYSFS/iostats
  # echo 0 > $SYSFS/rq_affinity
  # echo 2 > $SYSFS/nomerges
  # echo 0 > $SYSFS/io_poll_delay
done
