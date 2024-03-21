During publishing a corresponding archive is extracted to ./optional/libssl-1.1/ folder

# standalone lib ssl 1.1 installer
```
url=https://raw.githubusercontent.com/devizer/glist/master/install-libssl-1.1.sh; (wget -q -nv --no-check-certificate -O - $url 2>/dev/null || curl -ksSL $url) | bash
```