param([string]$Unity = '', [string]$ExistingFixture = '')
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$versionFile = Join-Path $root 'ProjectSettings/ProjectVersion.txt'
$version = (Select-String -LiteralPath $versionFile -Pattern '^m_EditorVersion: (.+)$').Matches[0].Groups[1].Value
if (!$Unity) { $Unity = "F:\UnityInstalls\$version\Editor\Unity.exe" }
if (!(Test-Path -LiteralPath $Unity)) { throw 'Pass the matching Unity editor with -Unity.' }
$fixture = Join-Path $root ('Temp/CameraUnity-' + [Guid]::NewGuid().ToString('N'))
if ($ExistingFixture) {
    $fixture = [IO.Path]::GetFullPath($ExistingFixture)
    $tempRoot = [IO.Path]::GetFullPath((Join-Path $root 'Temp')) + [IO.Path]::DirectorySeparatorChar
    if (!$fixture.StartsWith($tempRoot,[StringComparison]::OrdinalIgnoreCase) -or !((Test-Path -LiteralPath (Join-Path $fixture 'camera-fixture.marker')) -or (Test-Path -LiteralPath (Join-Path $fixture 'arena-fixture.marker')) -or (Test-Path -LiteralPath (Join-Path $fixture 'audio-fixture.marker')))) { throw 'Existing fixture must be a marked camera project inside repository Temp.' }
}
$existingProbe=Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" | Where-Object {$_.CommandLine -and $_.CommandLine.Contains($fixture)}
if($existingProbe){throw 'The isolated camera fixture is already open; finish that run before reusing it.'}
Write-Host "Full-game camera fixture: $fixture"
foreach ($folder in @('Assets','Packages','ProjectSettings','Library/PackageCache')) {
    New-Item -ItemType Directory -Force -Path (Join-Path $fixture $folder) | Out-Null
    & robocopy (Join-Path $root $folder) (Join-Path $fixture $folder) /E /COPY:DAT /R:1 /W:1 /NFL /NDL /NJH /NJS | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "Fixture copy failed: $folder" }
}
New-Item -ItemType Directory -Force -Path (Join-Path $fixture 'camera-mods') | Out-Null
& robocopy (Join-Path $root 'Mods/example.camera-lab') (Join-Path $fixture 'camera-mods/example.camera-lab') /E /COPY:DAT /R:1 /W:1 /NFL /NDL /NJH /NJS | Out-Null
if ($LASTEXITCODE -ge 8) { throw 'Mod fixture copy failed.' }

Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'CameraUnity.cs') -Destination (Join-Path $fixture 'Assets/Editor') -Force
[IO.File]::WriteAllText((Join-Path $fixture 'camera-fixture.marker'),'Isolated full-game native camera acceptance')
$log = Join-Path $fixture ('validation-'+[Guid]::NewGuid().ToString('N')+'.log')
Write-Host "Native camera log: $log"
$runStarted = [DateTime]::UtcNow
# Fresh profiles share only immutable hash-addressed TAR data on the project
# drive, avoiding a gigabyte of system-drive extraction on each acceptance run.
$cameraProduct = 'CameraUnity-' + [Guid]::NewGuid().ToString('N')
$cameraProfile = [IO.Path]::GetFullPath((Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) ('../LocalLow/EclipseAcceptance/'+$cameraProduct)))
$cameraCache = Join-Path $cameraProfile 'SF2DE/TarAssetCache'
$cameraCacheTarget = Join-Path $fixture 'audio-tar-cache'
New-Item -ItemType Directory -Force -Path $cameraCacheTarget | Out-Null
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $cameraCache) | Out-Null
New-Item -ItemType Junction -Path $cameraCache -Target $cameraCacheTarget | Out-Null
$process = Start-Process -FilePath $Unity -ArgumentList @('-projectPath',('"'+$fixture+'"'),'-executeMethod','CameraUnity.Run','-cameraAcceptanceProductName',$cameraProduct,'-logFile',('"'+$log+'"')) -WindowStyle Hidden -PassThru
Write-Host "Native camera process: $($process.Id)"
$deadline = [DateTime]::UtcNow.AddMinutes(15)
while (!$process.WaitForExit(20000)) {
    if ((Test-Path -LiteralPath $log) -and (Select-String -LiteralPath $log -Pattern 'error CS\d+:' -Quiet)) {$process.Kill();throw "Native camera fixture failed compilation: $log"}
    if ([DateTime]::UtcNow -gt $deadline) { $process.Kill(); throw "Native camera timed out: $log" }
}
Select-String -LiteralPath $log -Pattern '\[CameraUnity\]|error CS' | ForEach-Object { Write-Host $_.Line }
$result = Join-Path $fixture 'camera-result.txt'
if ($process.ExitCode -ne 0 -or !(Test-Path -LiteralPath $result) -or (Get-Item -LiteralPath $result).LastWriteTimeUtc -lt $runStarted) { throw "Native camera acceptance failed: $log" }
Get-Content -LiteralPath $result
