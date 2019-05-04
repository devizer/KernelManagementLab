## W3-Top
Is a web-based linux-bound monitor and benchmark UI. As of now w3-top it relies on `/proc` and `/sys` kernel-bound "filesystems". Mac OS and Windows are not supported.

Supported architectures are restricted by dotnet-sdk: x86_64, arm and arm64.

### Install from source
The easiest way to (re)install it as a SystemD service, namely w3top, is to build from source:

```bash
export HTTP_PORT=5050
export INSTALL_DIR=/opt/w3top
export RESPONSE_COMPRESSION=True
script=https://raw.githubusercontent.com/devizer/KernelManagementLab/master/build-w3-dashboard.sh
wget -q -nv --no-check-certificate -O - $script | bash -s reinstall_service 
```

Service's journal is available using traditional SystemD's journal:

```bash
journalctl -fu w3top.service
```

The build script above depends on dotnet sdk 2.2, nodejs 10.5+ (with yarn) and powershell:. Official portable versions of them can be installed using one-liner below:
```bash
script=https://raw.githubusercontent.com/devizer/glist/master/install-dotnet-and-nodejs.sh
wget -q -nv --no-check-certificate -O - $script | bash -s dotnet node pwsh
```

### Common .net core dependencies:
https://docs.microsoft.com/en-us/dotnet/core/linux-prerequisites

### Specific runtime Dependency: libMonoPosixHelper.so
Minimum version is **`4.6`**

Depending on runtime identifier this library may be missed on publish. 
Xamarin already prepared it for most of runtimes of dotnet core, but for for all.
It can be installed using:

```bash
# Alpine Linux
sudo apk add mono

# Fedora/CentOS/RedHat
sudo yum install mono-core

# Debian/Ubuntu derivatives
sudo apt install mono-runtime-common
```

### Unininstall
```bash
sudo systemctl disable w3top.service
sudo rm -f /etc/systemd/system/w3top.service 
sudo rm -rf /opt/w3top
```
