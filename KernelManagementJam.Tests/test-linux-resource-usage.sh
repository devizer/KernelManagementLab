script=https://raw.githubusercontent.com/devizer/glist/master/install-dotnet-and-nodejs.sh; (wget -q -nv --no-check-certificate -O - $script 2>/dev/null || curl -ksSL $script) | bash -s dotnet
cd ~
rm -rf KernelManagementLab || true
git clone https://github.com/devizer/KernelManagementLab
cd KernelManagementLab/KernelManagementJam.Tests

export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
git pull; time dotnet test --filter Usage_Tests | cat
