##### 1.7
- Added swap partitions and files for live Mounts page. Now Mounts page unites links from `/proc/swaps` and `/proc/mounts` sources:
![Mounts](https://github.com/devizer/KernelManagementLab/raw/master/images/Mounts-and-Swaps.png "mounts and swaps") 
##### 1.6 
- Added host info (name, os, cpu, ram) to the header
![Mounts](https://github.com/devizer/KernelManagementLab/raw/master/images/Networks-Live-Chart.png "Network metrics live chart")
##### 1.5 
- Bootstrap navigation replaced by Material UI drawler
##### 1.4 
- Publishing to cloud: http://w3-top-on-xeon.devizer.tech, http://w3-top-on-pi.devizer.tech
- Added customizable one line installer:
```bash
# Default values below ar shown for illustration
export HTTP_PORT=5050
export INSTALL_DIR=/opt/w3top
export RESPONSE_COMPRESSION=True
script=https://raw.githubusercontent.com/devizer/KernelManagementLab/master/build-w3-dashboard.sh
wget -q -nv --no-check-certificate -O - $script | bash -s reinstall_service 
```
 
##### 1.2
- Added live Mounts page
- Rewritten signalR's DataSource and its watchdog.
##### 1.1
- Implemented versioning with its injection into C# assembly info and AppGitInfo.json
##### 1.0
- Added Networks metrics live charts page
##### 0.2
- 2-Axis Chart prototype
##### 0.1
- C3 Chart prototype
