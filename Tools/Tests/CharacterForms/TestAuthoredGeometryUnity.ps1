param([string]$Unity = '', [string]$ExistingFixture = '')
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$versionFile = Join-Path $root 'ProjectSettings/ProjectVersion.txt'
$version = (Select-String -LiteralPath $versionFile -Pattern '^m_EditorVersion: (.+)$').Matches[0].Groups[1].Value
if (!$Unity) { $Unity = "F:\UnityInstalls\$version\Editor\Unity.exe" }
if (!(Test-Path -LiteralPath $Unity)) { throw 'Pass the matching Unity editor with -Unity.' }
$fixture = Join-Path $root ('Temp/AuthoredGeometryUnity-' + [Guid]::NewGuid().ToString('N'))
if ($ExistingFixture) {
    $fixture = [IO.Path]::GetFullPath($ExistingFixture)
    $tempRoot = [IO.Path]::GetFullPath((Join-Path $root 'Temp')) + [IO.Path]::DirectorySeparatorChar
    if (!$fixture.StartsWith($tempRoot,[StringComparison]::OrdinalIgnoreCase) -or !((Test-Path -LiteralPath (Join-Path $fixture 'authored-geometry-fixture.marker')) -or (Test-Path -LiteralPath (Join-Path $fixture 'fighter-playback-fixture.marker')))) { throw 'Existing fixture must be a marked geometry project inside repository Temp.' }
}
Write-Host "Full-game authored geometry fixture: $fixture"
if(Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" | Where-Object {$_.CommandLine -and $_.CommandLine.Contains($fixture)}){throw 'The isolated geometry project is already open.'}
foreach ($folder in @('Assets','Packages','ProjectSettings','Library/PackageCache')) {
    New-Item -ItemType Directory -Force -Path (Join-Path $fixture $folder) | Out-Null
    & robocopy (Join-Path $root $folder) (Join-Path $fixture $folder) /E /COPY:DAT /R:1 /W:1 /NFL /NDL /NJH /NJS | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "Fixture copy failed: $folder" }
}
New-Item -ItemType Directory -Force -Path (Join-Path $fixture 'geometry-empty-mods') | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'AuthoredGeometryUnity.cs') -Destination (Join-Path $fixture 'Assets/Editor') -Force
foreach($validation in Get-ChildItem -LiteralPath (Join-Path $root 'Tools/Tests') -Recurse -Filter '*.cs' -File){
    $destination=Join-Path $fixture ('Assets/Editor/'+$validation.Name)
    if(Test-Path -LiteralPath $destination){Copy-Item -LiteralPath $validation.FullName -Destination $destination -Force}
}
[IO.File]::WriteAllText((Join-Path $fixture 'authored-geometry-fixture.marker'),'Isolated full-game native geometry acceptance')
$log = Join-Path $fixture ('validation-'+[Guid]::NewGuid().ToString('N')+'.log')
Write-Host "Native authored geometry log: $log"
$runStarted = [DateTime]::UtcNow
# Fresh profiles share only immutable hash-addressed TAR data on the project
# drive, avoiding a gigabyte of system-drive extraction on each acceptance run.
$geometryProduct = 'AuthoredGeometryUnity-' + [Guid]::NewGuid().ToString('N')
$geometryProfile = [IO.Path]::GetFullPath((Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) ('../LocalLow/EclipseAcceptance/'+$geometryProduct)))
$geometryCache = Join-Path $geometryProfile 'SF2DE/TarAssetCache'
$geometryCacheTarget = Join-Path $fixture 'audio-tar-cache'
New-Item -ItemType Directory -Force -Path $geometryCacheTarget | Out-Null
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $geometryCache) | Out-Null
New-Item -ItemType Junction -Path $geometryCache -Target $geometryCacheTarget | Out-Null
$process = Start-Process -FilePath $Unity -ArgumentList @('-projectPath',('"'+$fixture+'"'),'-executeMethod','AuthoredGeometryUnity.Run','-geometryAcceptanceProductName',$geometryProduct,'-logFile',('"'+$log+'"')) -WindowStyle Hidden -PassThru
Write-Host "Native authored geometry process: $($process.Id)"
$deadline = [DateTime]::UtcNow.AddMinutes(15)
while (!$process.WaitForExit(20000)) {
    if((Test-Path -LiteralPath $log) -and (Select-String -LiteralPath $log -Pattern 'error CS\d+:' -Quiet)){$process.Kill();throw "Geometry fixture failed compilation: $log"}
    if ([DateTime]::UtcNow -gt $deadline) { $process.Kill(); throw "Native authored geometry timed out: $log" }
}
Select-String -LiteralPath $log -Pattern '\[AuthoredGeometryUnity\]|error CS' | ForEach-Object { Write-Host $_.Line }
$result = Join-Path $fixture 'authored-geometry-result.txt'
if ($process.ExitCode -ne 0 -or !(Test-Path -LiteralPath $result) -or (Get-Item -LiteralPath $result).LastWriteTimeUtc -lt $runStarted) { throw "Native authored geometry acceptance failed: $log" }
Get-Content -LiteralPath $result
