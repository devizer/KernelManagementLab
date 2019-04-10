# Install from source

1. Install build tools: dotnet-sdk, nodejs (with yarn) and powershell for Debian and derivatives, Fedora/CentOS/RedHat
```bash
wget -q -nv --no-check-certificate -O - https://raw.githubusercontent.com/devizer/glist/master/install-dotnet-and-nodejs.sh | bash -s dotnet node pwsh
```

2. Clone, build and (re)install w3top service as a SystemD unit

```bash
export HTTP_PORT=5050
export INSTALL_DIR=/opt/w3top
script=https://raw.githubusercontent.com/devizer/KernelManagementLab/master/build-w3-dashboard.sh
wget -q -nv --no-check-certificate -O - $script | bash -s reinstall_service 
```

3. Managing w3top.service:

```bash
# Manage service
systemctl enable/disable/start/restart/stop w3top
# following service logs
journalctl -fu w3top.service
```

# Runtime Dependency: libMonoPosixHelper.so
It requires libMonoPosixHelper.so for quering info about linux-kind filesystem metainfo.

For Fedora/CentOS/RedHat it can be installed using

```bash
yum install mono-core
```

For Debian/Ubuntu derivatives it can be installed using
```bash
apt install mono-runtime-common
```
