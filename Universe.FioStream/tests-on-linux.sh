work=/transient-builds/fio; test "$(uname -m)" == "Darwin" && work=~/build/fio; sudo mkdir -p $work; sudo chown -R $(whoami) $work; cd $work; git clone https://github.com/devizer/KernelManagementLab; cd KernelManagementLab; git pull; cd Universe.FioStream.Tests
rm -rf ~/.cache/fio
time dotnet test  --filter "FullyQualifiedName~Discovery_Fio_Features" -f net5.0 | tee test-discovery-fio-features.log
time dotnet test  --filter "FullyQualifiedName~Test_Download_Only_Linux" -f net5.0 | tee test-discovery-fio-features.log
time dotnet test -f net5.0 | tee tests-all.log
time dotnet test  --filter "FullyQualifiedName~Features_Tests" -f net5.0 | tee test-features.log
time dotnet test  --filter "FullyQualifiedName~FioEngineListTests" -f net5.0 | tee test-engine-list.log
time dotnet test  --filter "FullyQualifiedName~FioLauncherSmokeTests" -f net5.0 | tee test-launcher.log
time dotnet test -f net5.0 | tee tests-all.log

dotnet test  --filter "FullyQualifiedName~FioStreamReaderTests" -f netcoreapp2.2
