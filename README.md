### W3-Top
Is a web-based linux-bound monitor and benchmark UI. As of now w3-top it relays on `/proc` and `/sys` kernel-bound "filesystems". Mac OS and Windows are not supported.

Supported architectures are restricted by dotnet-sdk: x86_64, arm and arm64.

#### Install from source
The easiest way to (re)install it as a SystemD service, namely w3top, is to build from source:

```bash
export HTTP_PORT=5050
export INSTALL_DIR=/opt/w3top
script=https://raw.githubusercontent.com/devizer/KernelManagementLab/master/build-w3-dashboard.sh
wget -q -nv --no-check-certificate -O - $script | bash -s reinstall_service 
```

Service's journal is available using traditional SystemD's journal:

```bash
journalctl -fu w3top.service
```

The build script above depends on dotnet sdk 2.2, nodejs 10.5+ (with yarn) and powershell:. Portable versions of them can be installed using one-liner below:
```bash
wget -q -nv --no-check-certificate -O - https://raw.githubusercontent.com/devizer/glist/master/install-dotnet-and-nodejs.sh | bash -s dotnet node pwsh
```

##### Runtime Dependency: libMonoPosixHelper.so
W3-Top requires libMonoPosixHelper.so for querying info about linux-kind filesystems meta-info. It can be installed using:

```bash
# Debian/CentOS/RedHat
sudo yum install mono-core

# Debian/Ubuntu derivatives
sudo apt install mono-runtime-common
```
