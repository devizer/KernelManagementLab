work=$HOME/KernelManagementLab; \
cd $HOME; \
rm -rf $work; \
git clone https://github.com/devizer/KernelManagementLab; \
cd KernelManagementLab; \
 
git pull; \
nuget restore *.sln; \
xbuild /t:Rebuild /p:Configuration=Debug /v:m; \
cd LinuxNetStatLab/bin/Debug; \
pdb2mdb LinuxNetStatLab.exe; mono LinuxNetStatLab.exe


