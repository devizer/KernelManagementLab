if [[ "$(command -v dotnet)" == "" ]] || [[ "$(command -v git)" == "" ]]; then
  sudo apt update; sudo apt install git curl -y -q
  DOTNET_Url=https://dot.net/v1/dotnet-install.sh;
  curl -o /tmp/_dotnet-install.sh -ksSL $DOTNET_Url
  mkdir -p /transient-builds
  time sudo -E bash /tmp/_dotnet-install.sh -version 3.1.120 -i /transient-builds/dotnet-3.1
  export PATH="/transient-builds/dotnet-3.1:$PATH"
  dotnet --info
  unset MSBuildSDKsPath || true
fi
work=$HOME/build/KernelManagementJam.Tests
git clone https://github.com/devizer/KernelManagementLab $work
cd $work
git pull
cd KernelManagementJam.Tests
time dotnet test -f netcoreapp3.1 --filter "FullyQualifiedName ~ HugeCrossInfo_Tests"
