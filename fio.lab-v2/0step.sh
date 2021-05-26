work=/transient-builds/fio; mkdir -p $work; cd $work; git clone https://github.com/devizer/KernelManagementLab; cd KernelManagementLab; git pull; cd fio.lab-v2
rm -rf result; mkdir -p result; time bash rebuild-all.sh | tee result/details.log


