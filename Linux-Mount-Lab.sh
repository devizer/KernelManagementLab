work=$HOME/KernelManagementLab; \
cd $HOME; \
rm -rf $work; \
git clone https://github.com/devizer/KernelManagementLab; \
cd KernelManagementLab; \
 
git pull; \
xbuild /t:Rebuild /p:Configuration=Debug; \
cd MountLab/bin/Debug; \
pdb2mdb MountLab.exe; mono MountLab.exe


