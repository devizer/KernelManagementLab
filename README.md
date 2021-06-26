## W3-Top &nbsp;&nbsp;&nbsp;[![W3Top Stable Version](https://img.shields.io/github/v/release/devizer/KernelManagementLab?label=Stable)](https://github.com/devizer/w3top-bin/blob/master/README.md#reinstallation-of-precompiled-binaries)

Is a web-based linux-bound monitor and benchmark UI. Mac OS and Windows are not supported.

Supported CPU architectures are x86_64, armv7, and arm64.

### History and Screenshots
[WHATSNEW.md](https://github.com/devizer/KernelManagementLab/blob/master/WHATSNEW.md)

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

The build script above depends on dotnet sdk 3.1, nodejs 10+ (with yarn) and powershell. Official portable versions of them can be installed using one-liner below:
```bash
script=https://raw.githubusercontent.com/devizer/glist/master/install-dotnet-and-nodejs.sh
wget -q -nv --no-check-certificate -O - $script | bash -s dotnet node pwsh
```

### Optional .net core dependencies:
https://docs.microsoft.com/en-us/dotnet/core/linux-prerequisites

### Unininstall
```bash
sudo systemctl disable w3top.service
sudo rm -f /etc/systemd/system/w3top.service 
sudo rm -rf /opt/w3top
```



