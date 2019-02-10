work=$HOME/KernelManagementLab; \
cd $HOME; \
rm -rf $work; \
git clone https://github.com/devizer/KernelManagementLab; \
cd KernelManagementLab; \
 
git pull; \
nuget restore *.sln; \
xbuild /t:Rebuild /p:Configuration=Debug /v:m; \
cd MountLab/bin/Debug; \
pdb2mdb MountLab.exe; mono MountLab.exe

