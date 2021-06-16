#!/usr/bin/env bash
sudo mkdir -p /mnt/ramdisk
rm -rf /mnt/ramdisk/*
sudo mount -t tmpfs -o size=2048m tmpfs /mnt/ramdisk
# --group_reporting: cpu usage if for just 1 thread
for eng in posixaio libaio io_uring sync psync; do
    sudo fio --name=my --bs=4k --size=150M --iodepth=64 --numjobs=8 --gtod_reduce=1 \
         --ioengine=$eng \
         --directory=/mnt/ramdisk \
         --runtime=9 --time_based | tee engine-$eng.log
done
ls -la /mnt/ramdisk
rm -rf /mnt/ramdisk/*
sudo umount /mnt/ramdisk

echo 'TROUBLE
for seq access we need single thread and single file sized as working set
for random access using N threads we need N files sized as (working set / N)
' > /dev/null