DATE=`date +%Y-%m-%d-%H-%M-%S`
file=Kernel-Management-Core-$DATE.7z
echo FILE is $file
sudo mkdir -p /backups
sudo 7za a /backups/$file -m0=lzma -mx=9 -mfb=256 -md=256m -ms=on -mqs=on -xr!bin -xr!obj -xr!*.userprefs -xr!build -xr!node_modules -xr!.git -xr!packages -xr!package-lock.json -xr!yarn.lock .

