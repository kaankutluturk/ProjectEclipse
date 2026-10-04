param([string]$Unity = '', [string]$ExistingFixture = '')
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$versionFile = Join-Path $root 'ProjectSettings/ProjectVersion.txt'
$version = (Select-String -LiteralPath $versionFile -Pattern '^m_EditorVersion: (.+)$').Matches[0].Groups[1].Value
if (!$Unity) { $Unity = "F:\UnityInstalls\$version\Editor\Unity.exe" }
if (!(Test-Path -LiteralPath $Unity)) { throw 'Pass the matching Unity editor with -Unity.' }
$fixture = Join-Path $root ('Temp/RangedActorsUnity-' + [Guid]::NewGuid().ToString('N'))
if ($ExistingFixture) {
    $fixture = [IO.Path]::GetFullPath($ExistingFixture)
    $tempRoot = [IO.Path]::GetFullPath((Join-Path $root 'Temp')) + [IO.Path]::DirectorySeparatorChar
    if (!$fixture.StartsWith($tempRoot,[StringComparison]::OrdinalIgnoreCase) -or !((Test-Path -LiteralPath (Join-Path $fixture 'ranged-actors-fixture.marker')) -or (Test-Path -LiteralPath (Join-Path $fixture 'fighter-playback-fixture.marker')))) { throw 'Existing fixture must be a marked typed actor project inside repository Temp.' }
}
$existingProbe = Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" | Where-Object {
    $_.CommandLine -and $_.CommandLine.Contains($fixture)
}
if ($existingProbe) { throw 'The isolated project is already open in Unity. Finish its current run before reusing it.' }
Write-Host "Full-game typed actor fixture: $fixture"
foreach ($folder in @('Assets','Packages','ProjectSettings','Library/PackageCache')) {
    New-Item -ItemType Directory -Force -Path (Join-Path $fixture $folder) | Out-Null
    & robocopy (Join-Path $root $folder) (Join-Path $fixture $folder) /E /COPY:DAT /R:1 /W:1 /NFL /NDL /NJH /NJS | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "Fixture copy failed: $folder" }
}
New-Item -ItemType Directory -Force -Path (Join-Path $fixture 'actors-empty-mods') | Out-Null

New-Item -ItemType Directory -Force -Path (Join-Path $fixture 'ranged-actor-mods') | Out-Null
& robocopy (Join-Path $root 'Mods/example.ranged-actors') (Join-Path $fixture 'ranged-actor-mods/example.ranged-actors') /E /COPY:DAT /R:1 /W:1 /NFL /NDL /NJH /NJS | Out-Null
if ($LASTEXITCODE -ge 8) { throw 'Actor example fixture copy failed.' }

Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'RangedActorsUnity.cs') -Destination (Join-Path $fixture 'Assets/Editor') -Force
foreach ($validation in Get-ChildItem -LiteralPath (Join-Path $root 'Tools/Tests') -Recurse -Filter '*.cs' -File) {
    $copiedValidation = Join-Path $fixture ('Assets/Editor/' + $validation.Name)
    if (Test-Path -LiteralPath $copiedValidation) { Copy-Item -LiteralPath $validation.FullName -Destination $copiedValidation -Force }
}
[IO.File]::WriteAllText((Join-Path $fixture 'ranged-actors-fixture.marker'),'Isolated full-game native typed actor acceptance')
$log = Join-Path $fixture ('validation-'+[Guid]::NewGuid().ToString('N')+'.log')
Write-Host "Native typed actor log: $log"
$runStarted = [DateTime]::UtcNow
# Fresh profiles share only immutable hash-addressed TAR data on the project
# drive, avoiding a gigabyte of system-drive extraction on each acceptance run.
$actorProbeProduct = 'RangedActorsUnity-' + [Guid]::NewGuid().ToString('N')
$actorProbeProfile = [IO.Path]::GetFullPath((Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) ('../LocalLow/EclipseAcceptance/'+$actorProbeProduct)))
$actorProbeCache = Join-Path $actorProbeProfile 'SF2DE/TarAssetCache'
$actorProbeCacheTarget = Join-Path $fixture 'audio-tar-cache'
New-Item -ItemType Directory -Force -Path $actorProbeCacheTarget | Out-Null
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $actorProbeCache) | Out-Null
New-Item -ItemType Junction -Path $actorProbeCache -Target $actorProbeCacheTarget | Out-Null
$arguments = @('-projectPath',('"'+$fixture+'"'),'-executeMethod','RangedActorsUnity.Run','-actorAcceptanceProductName',$actorProbeProduct,'-logFile',('"'+$log+'"'))
$process = Start-Process -FilePath $Unity -ArgumentList $arguments -WindowStyle Hidden -PassThru
Write-Host "Native typed actor process: $($process.Id)"
$deadline = [DateTime]::UtcNow.AddMinutes(15)
while (!$process.WaitForExit(20000)) {
    if ((Test-Path -LiteralPath $log) -and (Select-String -LiteralPath $log -Pattern 'error CS\d+:' -Quiet)) {
        $process.Kill(); throw "Native typed actor fixture failed compilation: $log"
    }
    if ([DateTime]::UtcNow -gt $deadline) { $process.Kill(); throw "Native typed actor timed out: $log" }
}
Select-String -LiteralPath $log -Pattern '\[RangedActorsUnity\]|error CS' | ForEach-Object { Write-Host $_.Line }
$result = Join-Path $fixture 'ranged-actors-result.txt'
if ($process.ExitCode -ne 0 -or !(Test-Path -LiteralPath $result) -or (Get-Item -LiteralPath $result).LastWriteTimeUtc -lt $runStarted) { throw "Native typed actor acceptance failed: $log" }
Get-Content -LiteralPath $result
