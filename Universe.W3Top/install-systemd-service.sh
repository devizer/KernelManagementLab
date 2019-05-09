#!/usr/bin/env bash
dotnet=$(command -v dotnet)
set -e
set -u
pushd `dirname $0` > /dev/null; scriptpath=`pwd`; popd > /dev/null
if [[ ! -f "$scriptpath/Universe.W3Top" ]]; then echo ERROR: publish the project first; exit 1; fi
HTTP_PORT="${HTTP_PORT:-5050}"
RESPONSE_COMPRESSION="${RESPONSE_COMPRESSION:-True}"

ver=$("$scriptpath/Universe.W3Top" --version)
echo Configuring w3top service $ver located at $scriptpath for 'http://<ip|name>:'$HTTP_PORT listener

sudo systemctl stop w3top    >/dev/null 2>&1 || true
sudo systemctl disable w3top >/dev/null 2>&1 || true

echo '
[Unit]
Description=W3Top service.
Documentation=https://github.com/devizer/KernelManagementLab
After=network.target

[Service]
Type=simple
PIDFile=/var/run/w3top.pid
WorkingDirectory='$scriptpath'
ExecStart='$scriptpath'/Universe.W3Top
Restart=on-failure
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=w3top
User=root
Environment=PID_FILE_FULL_PATH=/var/run/w3top.pid
Environment=ASPNETCORE_URLS=http://0.0.0.0:'$HTTP_PORT'
Environment=FORCE_HTTPS_REDIRECT=False
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment=DUMPS_ARE_ENABLED=False
Environment=RESPONSE_COMPRESSION='$RESPONSE_COMPRESSION'
Environment=BLOCK_DEVICE_VISIBILITY_THRESHOLD=2048
Environment=INSTALL_DIR='$scriptpath'

[Install]
WantedBy=multi-user.target
' | sudo tee /etc/systemd/system/w3top.service >/dev/null

sudo systemctl enable w3top
sudo systemctl daemon-reload || true
sudo systemctl start w3top
# sudo journalctl -fu w3top.service
