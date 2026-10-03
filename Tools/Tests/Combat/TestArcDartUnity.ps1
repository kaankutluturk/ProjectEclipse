param([string]$Unity = '', [string]$ExistingFixture = '')
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$versionFile = Join-Path $root 'ProjectSettings/ProjectVersion.txt'
$version = (Select-String -LiteralPath $versionFile -Pattern '^m_EditorVersion: (.+)$').Matches[0].Groups[1].Value
if (!$Unity) { $Unity = "F:\UnityInstalls\$version\Editor\Unity.exe" }
if (!(Test-Path -LiteralPath $Unity)) { throw 'Pass the matching Unity editor with -Unity.' }
$fixture = Join-Path $root ('Temp/ArcDartUnity-' + [Guid]::NewGuid().ToString('N'))
if ($ExistingFixture) {
    $fixture = [IO.Path]::GetFullPath($ExistingFixture)
    $tempRoot = [IO.Path]::GetFullPath((Join-Path $root 'Temp')) + [IO.Path]::DirectorySeparatorChar
    if (!$fixture.StartsWith($tempRoot,[StringComparison]::OrdinalIgnoreCase) -or !((Test-Path -LiteralPath (Join-Path $fixture 'arc-dart-fixture.marker')) -or (Test-Path -LiteralPath (Join-Path $fixture 'fighter-playback-fixture.marker')))) { throw 'Existing fixture must be a marked projectile project inside repository Temp.' }
}
Write-Host "Full-game projectile fixture: $fixture"
foreach ($folder in @('Assets','Packages','ProjectSettings','Library/PackageCache')) {
    New-Item -ItemType Directory -Force -Path (Join-Path $fixture $folder) | Out-Null
    & robocopy (Join-Path $root $folder) (Join-Path $fixture $folder) /E /COPY:DAT /R:1 /W:1 /NFL /NDL /NJH /NJS | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "Fixture copy failed: $folder" }
}
New-Item -ItemType Directory -Force -Path (Join-Path $fixture 'projectile-mods') | Out-Null
& robocopy (Join-Path $root 'Mods/example.arc-dart') (Join-Path $fixture 'projectile-mods/example.arc-dart') /E /COPY:DAT /R:1 /W:1 /NFL /NDL /NJH /NJS | Out-Null
if ($LASTEXITCODE -ge 8) { throw 'Mod fixture copy failed.' }

Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'ArcDartUnity.cs') -Destination (Join-Path $fixture 'Assets/Editor') -Force
[IO.File]::WriteAllText((Join-Path $fixture 'arc-dart-fixture.marker'),'Isolated full-game native projectile acceptance')
$log = Join-Path $fixture ('validation-'+[Guid]::NewGuid().ToString('N')+'.log')
Write-Host "Native projectile log: $log"
$runStarted = [DateTime]::UtcNow
# Fresh profiles share only immutable hash-addressed TAR data on the project
# drive, avoiding a gigabyte of system-drive extraction on each acceptance run.
$projectileProduct = 'ArcDartUnity-' + [Guid]::NewGuid().ToString('N')
$projectileProfile = [IO.Path]::GetFullPath((Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) ('../LocalLow/EclipseAcceptance/'+$projectileProduct)))
$projectileCache = Join-Path $projectileProfile 'SF2DE/TarAssetCache'
$projectileCacheTarget = Join-Path $fixture 'audio-tar-cache'
New-Item -ItemType Directory -Force -Path $projectileCacheTarget | Out-Null
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $projectileCache) | Out-Null
New-Item -ItemType Junction -Path $projectileCache -Target $projectileCacheTarget | Out-Null
$process = Start-Process -FilePath $Unity -ArgumentList @('-projectPath',('"'+$fixture+'"'),'-executeMethod','ArcDartUnity.Run','-arcDartAcceptanceProductName',$projectileProduct,'-logFile',('"'+$log+'"')) -WindowStyle Hidden -PassThru
Write-Host "Native projectile process: $($process.Id)"
$deadline = [DateTime]::UtcNow.AddMinutes(15)
while (!$process.WaitForExit(20000)) {
    if ([DateTime]::UtcNow -gt $deadline) { $process.Kill(); throw "Native projectile timed out: $log" }
}
Select-String -LiteralPath $log -Pattern '\[ArcDartUnity\]|error CS' | ForEach-Object { Write-Host $_.Line }
$result = Join-Path $fixture 'arc-dart-result.txt'
if ($process.ExitCode -ne 0 -or !(Test-Path -LiteralPath $result) -or (Get-Item -LiteralPath $result).LastWriteTimeUtc -lt $runStarted) { throw "Native projectile acceptance failed: $log" }
Get-Content -LiteralPath $result
