#!/usr/bin/env bash
dotnet=$(command -v dotnet)
pushd `dirname $0` > /dev/null; scriptpath=`pwd`; popd > /dev/null
echo Installing w3top service located at $scriptpath
echo '
[Unit]
Description=W3Top service. 
Documentation=https://github.com/devizer/KernelManagementLab
After=network.target

[Service]
Type=simple
PIDFile=/var/run/w3top.pid
WorkingDirectory='$scriptpath'
ExecStart='$dotnet' '$scriptpath'/Universe.W3Top.dll
Restart=on-failure
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=w3top
User=root
Environment=PID_FILE_FULL_PATH=/var/run/w3top.pid
Environment=ASPNETCORE_URLS=http://0.0.0.0:5050
Environment=FORCE_HTTPS_REDIRECT=False
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment=DUMPS_ARE_ENABLED=False

[Install]
WantedBy=multi-user.target
' | sudo tee /etc/systemd/system/w3top.service >/dev/null

sudo systemctl stop w3top    >/dev/null 2>&1 || true
sudo systemctl disable w3top >/dev/null 2>&1 || true
sudo systemctl enable w3top
sudo systemctl daemon-reload
sudo systemctl start w3top
sudo journalctl -fu w3top.service
