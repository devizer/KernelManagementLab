work=$HOME/KernelManagementLab
cd $HOME
rf -rf work
git clone https://github.com/devizer/KernelManagementLab
cd KernelManagementLab
git pull
cd LinuxNetStatLab
msbuild /t:Rebuild /p:Configuration=Debug
cd bin/Debug
mono LinuxNetStatLab.exe
