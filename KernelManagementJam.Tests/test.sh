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
cd ~
rm -rf KernelManagementLab || true
git clone https://github.com/devizer/KernelManagementLab
cd KernelManagementLab/KernelManagementJam.Tests

export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
git pull; time dotnet test -f netcoreapp3.1 
