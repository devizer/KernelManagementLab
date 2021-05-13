work=/transient-builds/fio; sudo mkdir -p $work; sudo chown -R $(whoami) $work; cd $work; git clone https://github.com/devizer/KernelManagementLab; cd KernelManagementLab; git pull; cd Universe.FioStream.Tests
rm -rf ~/.local/bin/state
time dotnet test -f net5.0 > tests-all.log 2>&1
time dotnet test -f net5.0 | tee tests-all.log
time dotnet test  --filter "FullyQualifiedName~Discovery_Fio_Features" -f net5.0 | tee test-discovery-fio-features.log
time dotnet test  --filter "FullyQualifiedName~Features_Tests" -f net5.0 | tee test-features.log
time dotnet test  --filter "FullyQualifiedName~FioEngineListTests" -f net5.0 | tee test-engine-list.log
time dotnet test  --filter "FullyQualifiedName~FioLauncherSmokeTests" -f net5.0 | tee test-launcher.log
time dotnet test -f net5.0 | tee tests-all.log

