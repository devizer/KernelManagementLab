work=/transient-builds/fio; mkdir -p $work; cd $work; git clone https://github.com/devizer/KernelManagementLab; cd KernelManagementLab; git pull; cd Universe.FioStream.Tests
time dotnet test -f net5.0

