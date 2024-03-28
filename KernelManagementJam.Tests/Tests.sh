for testprj in $(ls -1 | grep -E "\.Tests$"); do
  if [[ $testprj == "JsonLab.Tests" ]]; then continue; fi
  Say $testprj
  cd $testprj
  # (time try-and-retry nuget restore *.csproj) |& tee ../$testprj.Restore.Log
  (time msbuild /t:Restore,Build /p:Configuration=Release) |& tee ../$testprj.Build.Log
  cd ..
done
