work=$HOME/KernelManagementLab; \
cd $HOME; \
rm -rf $work; \
git clone https://github.com/devizer/KernelManagementLab; \
cd KernelManagementLab; 
xbuild /t:Rebuild /p:Configuration=Debug; \
git pull; \
cd LinuxNetStatLab; \
cd bin/Debug; pdb2mdb LinuxNetStatLab.exe; mono LinuxNetStatLab.exe 538


