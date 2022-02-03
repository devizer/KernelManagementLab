#!/usr/bin/env bash
dotnet=$(command -v dotnet)
set -e
set -u
pushd `dirname $0` > /dev/null; ScriptPath=`pwd`; popd > /dev/null

if [[ ! -f "$ScriptPath/Universe.W3Top" ]]; then echo ERROR: publish the project first; exit 1; fi
HTTP_PORT="${HTTP_PORT:-5050}"
HTTP_HOST="${HTTP_HOST:-0.0.0.0}"
RESPONSE_COMPRESSION="${RESPONSE_COMPRESSION:-True}"
DUMPS_ARE_ENABLED="${DUMPS_ARE_ENABLED:-False}"

MYSQL_DATABASE_ESCAPED="${MYSQL_DATABASE:-}"
PGSQL_DATABASE_ESCAPED="${PGSQL_DATABASE:-}"
MSSQL_DATABASE_ESCAPED="${MSSQL_DATABASE:-}"
if [[ "$(command -v systemd-escape)" != "" ]]; then
  MYSQL_DATABASE_ESCAPED=$(systemd-escape "${MYSQL_DATABASE:-}") || MYSQL_DATABASE_ESCAPED="${MYSQL_DATABASE:-}"
  PGSQL_DATABASE_ESCAPED=$(systemd-escape "${PGSQL_DATABASE:-}") || PGSQL_DATABASE_ESCAPED="${PGSQL_DATABASE:-}"
  MSSQL_DATABASE_ESCAPED=$(systemd-escape "${MSSQL_DATABASE:-}") || MSSQL_DATABASE_ESCAPED="${MSSQL_DATABASE:-}"
fi
   

export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
ver=$("$ScriptPath/Universe.W3Top" --version)
echo Configuring w3top service $ver located at ${ScriptPath} for 'http://<ip|name>:'${HTTP_PORT} listener

# Checking for SystemD & Sys V Init
hasUpdateRc=""; hasChkConfig=""; hasSystemCtl=""; hasJournalProcess=""; hasSystemD="";
command -v update-rc.d >/dev/null && hasUpdateRc=true || true
command -v chkconfig >/dev/null && hasChkConfig=true || true
# V1. service name matters, works
# systemctl is-active systemd-journald.service >/dev/null 2>&1 && hasSystemD=true || true
# V2. needs psproc
command -v systemctl >/dev/null && hasSystemCtl=true || true
pgrep systemd-journal >/dev/null 2>&1 && hasJournalProcess=true || true
if [[ -n "$hasSystemCtl" ]] && [[ -n "$hasJournalProcess" ]]; then hasSystemD=true; fi
# v3. the wrong
# systemctl is-system-running >/dev/null 2>&1 && hasSystemD=true || true


function Install_SystemD_Service() {
sudo systemctl stop w3top    >/dev/null 2>&1 || true
sudo systemctl disable w3top >/dev/null 2>&1 || true

echo '
[Unit]
Description=W3Top service
Documentation=https://github.com/devizer/KernelManagementLab
After=network.target

[Service]
Type=simple
PIDFile=/var/run/w3top.pid
WorkingDirectory='$ScriptPath'
ExecStart='$ScriptPath'/Universe.W3Top
Restart=on-failure
RestartSec=10
KillSignal=SIGINT
ExecStopPost='${ExecStopPost:-}'
SyslogIdentifier=w3top
User=root
Environment=PID_FILE_FULL_PATH=/var/run/w3top.pid
Environment=ASPNETCORE_URLS=http://'$HTTP_HOST':'$HTTP_PORT'
Environment=HTTP_PORT='$HTTP_PORT'
Environment=HTTP_HOST='$HTTP_HOST'
Environment=INSTALL_DIR='$ScriptPath'
Environment=RESPONSE_COMPRESSION='$RESPONSE_COMPRESSION'
Environment=BLOCK_DEVICE_VISIBILITY_THRESHOLD=2048
Environment=FORCE_HTTPS_REDIRECT=False
Environment=MYSQL_DATABASE='${MYSQL_DATABASE_ESCAPED:-}'
Environment=PGSQL_DATABASE='${PGSQL_DATABASE_ESCAPED:-}'
Environment=MSSQL_DATABASE='${MSSQL_DATABASE_ESCAPED:-}'
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment=DUMPS_ARE_ENABLED='${DUMPS_ARE_ENABLED:-False}'
Environment=DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
Environment=LD_LIBRARY_PATH='${APP_LD_LIBRARY_PATH:-}'

[Install]
WantedBy=multi-user.target
' | sudo tee /etc/systemd/system/w3top.service >/dev/null

sudo systemctl enable w3top
sudo systemctl daemon-reload || true
sudo systemctl start w3top
# sudo journalctl -fu w3top.service
}

function Install_SysVInit_Service() {
echo '#!/usr/bin/env bash

### BEGIN INIT INFO
# Provides:          w3top
# Required-Start:    $remote_fs $syslog
# Required-Stop:     $remote_fs $syslog
# Default-Start:     2 3 4 5
# Default-Stop:      0 1 6
# Short-Description: w3top
# Description:       w3top service
### END INIT INFO

export PATH="'$PATH'"
export PID_FILE_FULL_PATH=/var/run/w3top.pid
export ASPNETCORE_URLS=http://'$HTTP_HOST':'$HTTP_PORT'
export HTTP_PORT='$HTTP_PORT'
export HTTP_HOST='$HTTP_HOST'
export INSTALL_DIR='"'"$ScriptPath"'"'
export RESPONSE_COMPRESSION='$RESPONSE_COMPRESSION'
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
export FORCE_HTTPS_REDIRECT=False
export MYSQL_DATABASE='"'"${MYSQL_DATABASE:-}"'"'
export PGSQL_DATABASE='"'"${PGSQL_DATABASE:-}"'"'
export ASPNETCORE_ENVIRONMENT=Production
export DOTNET_PRINT_TELEMETRY_MESSAGE=false
export DUMPS_ARE_ENABLED='${DUMPS_ARE_ENABLED:-False}'
export BLOCK_DEVICE_VISIBILITY_THRESHOLD=2048
export LD_LIBRARY_PATH='${APP_LD_LIBRARY_PATH:-}'

case "$1" in
  start)
    echo "Starting w3top"
    cd "'${ScriptPath}'"
    mkdir -p /tmp
    (nohup "'${ScriptPath}'/Universe.W3Top" 1>/tmp/w3top.log 2>&1 ) &
    ;;
  stop)
    echo "Stopping w3top"
    pkill Universe.W3Top 1>/dev/null 2>&1 || true
    ;;
  version)
    # mainly used for tests on travis-ci
    "'${ScriptPath}'/Universe.W3Top" --version || exit 2;
    ;;
  *)
    echo "Usage: /etc/init.d/w3top {start|stop}"
    exit 1
    ;;
esac

exit 0
' | sudo tee /etc/init.d/w3top >/dev/null
sudo chmod +x /etc/init.d/w3top

if [ -n "$hasUpdateRc" ]; then
  # debian derivaties
  echo "Configuring /etc/init.d/w3top init-script using update-rc.d tool"
  sudo /etc/init.d/w3top stop >/dev/null 2>&1 || true
  sudo update-rc.d -f w3top remove || true
  sudo update-rc.d w3top defaults
  sudo /etc/init.d/w3top start
  sleep 1
elif [ -n "$hasChkConfig" ]; then
  # suse and redhat derivates
  echo "Configuring /etc/init.d/w3top init-script using chkconfig tool"
  sudo /etc/init.d/w3top stop >/dev/null 2>&1 || true
  sudo chkconfig --level 2345 w3top off || true
  sudo chkconfig --level 2345 w3top on
  sleep 1
  sudo /etc/init.d/w3top start
  sleep 3
else
  sudo /etc/init.d/w3top stop >/dev/null 2>&1 || true
  sleep 1
  sudo /etc/init.d/w3top start
  echo "Unable to configure w3top startup on system boot. The system should support one of these command:
  update-rc.d
  or chkconfig
  or systemctl (preferred)
  
To start|stop w3top service manually:
/etc/init.d/w3top start|stop"
  sleep 3
fi
}

# IGNORE_SYSTEMD is for testing only, don't use it ever
if [[ -n "$hasSystemD" ]] && [[ -z "${IGNORE_SYSTEMD:-}" ]]; then
  echo Installing SystemD service: /etc/systemd/system/w3top.service
  Install_SystemD_Service
else
  echo Installing Sys V Init service: /etc/init.d/w3top
  Install_SysVInit_Service
fi
