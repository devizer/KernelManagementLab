work=$HOME/KernelManagementLab
cd $HOME
rm -rf $work
git clone https://github.com/devizer/KernelManagementLab
cd KernelManagementLab
git pull
cd LinuxNetStatLab
msbuild /t:Rebuild /p:Configuration=Debug; cd bin/Debug; pdb2mdb LinuxNetStatLab.exe; mono LinuxNetStatLab.exe 538


