
echo '
Acquire::Check-Valid-Until "0";
' > /etc/apt/apt.conf.d/10no--check-valid-until

echo '
APT::Get::Assume-Yes "true";
' > /etc/apt/apt.conf.d/11assume-yes

echo '
APT::Get::AllowUnauthenticated "true";
' > /etc/apt/apt.conf.d/12allow-unauth

apt-get update
apt-get install git htop mc lsof ncdu -y


if [[ "$(command -v dotnet)" == "" ]]; then
  url=https://raw.githubusercontent.com/devizer/glist/master/install-dotnet-dependencies.sh; (wget -q -nv --no-check-certificate -O - $url 2>/dev/null || curl -ksSL $url) | bash
  script=https://raw.githubusercontent.com/devizer/glist/master/install-dotnet-and-nodejs.sh; (wget -q -nv --no-check-certificate -O - $script 2>/dev/null || curl -ksSL $script) | bash -s dotnet node pwsh
fi


work=$HOME/JsonLab
rm -rf $work; mkdir -p $work; cd $work
git clone https://github.com/devizer/KernelManagementLab
cd KernelManagementLab/JsonLab
dotnet run -f netcoreapp2.2 -c Release