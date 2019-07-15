dotnet --info >nul 2>&1
if errorlevel 1 (
  @"%SystemRoot%\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -InputFormat None -ExecutionPolicy Bypass -Command "iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))" 
  SET "PATH=%PATH%;%ALLUSERSPROFILE%\chocolatey\bin"
  choco install git kdiff3 nodejs-lts yarn dotnetcore-sdk netfx-4.7.2-devpack 7zip dbgview far visualstudio2019community visualstudio2019-workload-manageddesktop -my
)
call RefreshEnv.cmd

cd "%LOCALAPPDATA%"
set work=%LOCALAPPDATA%\JsonLab
rd /q /s "%work%"
mkdir "%work%" >nul 2>&1
cd "%work%"
git clone https://github.com/devizer/KernelManagementLab
cd KernelManagementLab/JsonLab
dotnet run -f netcoreapp2.2 -c Release
