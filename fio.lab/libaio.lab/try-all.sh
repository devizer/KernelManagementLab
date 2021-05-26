# work=/transient-builds/fio; sudo mkdir -p $work; sudo chown -R $(whoami) $work; cd $work; git clone https://github.com/devizer/KernelManagementLab; cd KernelManagementLab; git pull; cd fio.lab/libaio.lab; bash try-all.sh
work_base=/transient-builds/libaio-src
cat ver-links.txt | while read -r ver link; do
    echo "$ver" "$link"
done 