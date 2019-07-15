##### 1.17
- Added support with logic tests and installer tests for MySQL (5.1 and above) and Postgresql (8.4 and above) for metrics and benchmarks history.
- Improved usability for disk benchmark
<img src="https://github.com/devizer/KernelManagementLab/raw/master/images/DiskBenchmark-V1.gif" width="803px" height="562px" Alt="disk benchmark screencast" Title="disk benchmark screencast"></img>
 
##### 1.16
- Added disk benchmark for readonly volumes such as squashfs and dvd disks

##### 1.15
- The first disk benchmark is here:
<img src="https://github.com/devizer/KernelManagementLab/raw/master/images/Disk-Benchmark-V1.15.png" width="718px" Alt="The first disk's benchmark" Title="The first disk's benchmark"></img>

##### 1.14
- Added support for legacy Sys V Init service manager. SystemD is preferred if also present.

##### 1.13
- Implemented Disk Benchmark with command line interface and O_DIRECT support

##### 1.12.431
- Implemented Queue and Busy charts for disks
<img src="https://github.com/devizer/KernelManagementLab/raw/master/images/Disks-Live-Chart.png" width="516px" Alt="Disks metrics live chart" Title="Disks metrics live chart"></img>

##### 1.11.424
- Implemented installation using precompiled binaries: [install-w3top-service.sh](https://github.com/devizer/w3top-bin#reinstallation-of-precompiled-binaries)
- Added newVer property to BriefInfo response and broadcast message

##### 1.10.416
- Muted chart updating when app is minimized, on background tab or screen is locked. Fixed memory leaks on unmount

##### 1.9
- Improved header load time via new BriefInfo API method

##### 1.8
- Added Disks live charts without queue length and busy metrics

##### 1.7
- Added swap partitions and files for live Mounts page. Now Mounts page unites references from `/proc/swaps` and `/proc/mounts` sources:
<img src="https://github.com/devizer/KernelManagementLab/raw/master/images/Mounts-and-Swaps-v2.png" width="882px" title="Mounts and Swaps" alt="Mounts and Swaps"></img>

##### 1.6 
- Added host info (name, os, cpu, ram) to the header
<img src="https://github.com/devizer/KernelManagementLab/raw/master/images/Networks-Live-Chart.png" width="532px" alt="Network metrics live chart" title="Network metrics live chart"></img>

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
