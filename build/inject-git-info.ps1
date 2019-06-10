#!/usr/bin/env pwsh

function SaveAsJson { 
  param([object]$anObject, [string]$fileName) 
  $unixContent = ($anObject | ConvertTo-Json -Depth 99).Replace("`r", "")
  $Utf8NoBomEncoding = New-Object System.Text.UTF8Encoding $False
  [System.IO.File]::WriteAllLines($fileName, $unixContent, $Utf8NoBomEncoding)
}

function GetVersion {
  $ver = Get-Content "version.txt"
  return $ver
}

function IncrementBuild {
  $build = Get-Content "build.txt"
  $build = 1 + [long] $build
  SaveContent $build "build.txt"
  return $build
}

function SaveContent {
  param([object]$content, [string]$fileName) 
  $Utf8NoBomEncoding = New-Object System.Text.UTF8Encoding $False
  [System.IO.File]::WriteAllLines($fileName, $content, $Utf8NoBomEncoding)

}


# AssemblyGitInfo.cs
$branch = & { git rev-parse --abbrev-ref HEAD }
"Branch: [$branch]"

$commitsRaw = & { set TZ=GMT; git log -n 999999 --date=raw --pretty=format:"%cd" }
$lines = $commitsRaw.Split([Environment]::NewLine)
$commitCount = $lines.Length
$commitDate = $lines[0].Split(" ")[0]
"Commit Counter: [$commitCount]"
"Commit Date: [$commitDate]"

"[assembly: Universe.AssemblyGitInfo(`"$branch`", $commitCount, $($commitDate)L)]" > AssemblyGitInfo.cs

# AssemblyInfo.cs
$version = GetVersion
$build = IncrementBuild
"[assembly: System.Reflection.AssemblyVersion(`"$version.$commitCount.$build`")]" > AssemblyVersion.cs

# AppGitInfo.json
$jsonInfo = @{
  Branch = $branch;
  CommitCount = $commitCount;
  CommitDate = [long]$commitDate;
  BuildDate = [long] ([System.DateTime]::UtcNow - (new-object System.DateTime 1970, 1, 1, 0, 0, 0, ([DateTimeKind]::Utc) )).TotalSeconds;
  Version = "$version.$commitCount.$build"
}
SaveAsJson $jsonInfo "AppGitInfo.json"
Write-Host (Get-Content "AppGitInfo.json")
Copy-Item "AppGitInfo.json" ..\Universe.W3Top\ClientApp\src\AppGitInfo.json -Force

foreach($t in @("OfflinePrepare", "WindowsBootAnalyzer")) {
  # cp "*.cs" "..\sources\$t\Properties\"
}

# BinTray
$binTray = Get-Content "../bintray.json" | ConvertFrom-Json
Write-Host "Old .Version.Name $($binTray.version.name)"
Write-Host "Old .version.desc $($binTray.version.desc)"
$binTray.version.name = "$version.$commitCount.$build"
$binTray.version.desc = "Build $version.$commitCount.$build"
SaveAsJson $binTray "../bintray.json"

