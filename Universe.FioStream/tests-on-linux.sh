work=/transient-builds/fio; sudo mkdir -p $work; sudo chown -R $(whoami) $work; cd $work; git clone https://github.com/devizer/KernelManagementLab; cd KernelManagementLab; git pull; cd Universe.FioStream.Tests
time dotnet test -f net5.0 | tee tests.log

