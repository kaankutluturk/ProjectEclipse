param([Parameter(Mandatory=$true)][string]$Package, [string]$Unity = '', [string]$ExistingFixture = '', [switch]$OpponentOnly)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))
$versionFile = Join-Path $root 'ProjectSettings/ProjectVersion.txt'
$version = (Select-String -LiteralPath $versionFile -Pattern '^m_EditorVersion: (.+)$').Matches[0].Groups[1].Value
if (!$Unity) { $Unity = "F:\UnityInstalls\$version\Editor\Unity.exe" }
if (!(Test-Path -LiteralPath $Unity)) { throw 'Pass the matching Unity editor with -Unity.' }
$fixture = Join-Path $root ('Temp/PackagedCharacterUnity-' + [Guid]::NewGuid().ToString('N'))
if ($ExistingFixture) {
    $fixture = [IO.Path]::GetFullPath($ExistingFixture)
    $tempRoot = [IO.Path]::GetFullPath((Join-Path $root 'Temp')) + [IO.Path]::DirectorySeparatorChar
    if (!$fixture.StartsWith($tempRoot,[StringComparison]::OrdinalIgnoreCase) -or !((Test-Path -LiteralPath (Join-Path $fixture 'packaged-character-fixture.marker')) -or (Test-Path -LiteralPath (Join-Path $fixture 'text-input-fixture.marker')) -or (Test-Path -LiteralPath (Join-Path $fixture 'camera-fixture.marker')) -or (Test-Path -LiteralPath (Join-Path $fixture 'arena-fixture.marker')) -or (Test-Path -LiteralPath (Join-Path $fixture 'audio-fixture.marker')))) { throw 'Existing fixture must be a marked packaged-character project inside repository Temp.' }
}
$existingProbe=Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" | Where-Object {$_.CommandLine -and $_.CommandLine.Contains($fixture)}
if($existingProbe){throw 'The isolated packaged-character fixture is already open; finish that run before reusing it.'}
Write-Host "Full-game packaged-character fixture: $fixture"
foreach ($folder in @('Assets','Packages','ProjectSettings','Library/PackageCache')) {
    New-Item -ItemType Directory -Force -Path (Join-Path $fixture $folder) | Out-Null
    & robocopy (Join-Path $root $folder) (Join-Path $fixture $folder) /E /COPY:DAT /R:1 /W:1 /NFL /NDL /NJH /NJS | Out-Null
    if ($LASTEXITCODE -ge 8) { throw "Fixture copy failed: $folder" }
}
$packagePath=(Resolve-Path -LiteralPath $Package).Path
$characterModId=Split-Path -Leaf $packagePath
if($characterModId -notmatch '^[a-z0-9][a-z0-9_.-]{0,127}$' -or $characterModId -in @('core','sf2de')){throw 'Pass a generated package directory named after its mod ID.'}
$characterMods=Join-Path $fixture ('packaged-character-mods-'+[Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $characterMods | Out-Null
& robocopy $packagePath (Join-Path $characterMods $characterModId) /E /COPY:DAT /R:1 /W:1 /NFL /NDL /NJH /NJS | Out-Null
if ($LASTEXITCODE -ge 8) { throw 'Mod fixture copy failed.' }

Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'PackagedCharacterUnity.cs') -Destination (Join-Path $fixture 'Assets/Editor') -Force
[IO.File]::WriteAllText((Join-Path $fixture 'packaged-character-fixture.marker'),'Isolated full-game native packaged-character acceptance')
$log = Join-Path $fixture ('validation-'+[Guid]::NewGuid().ToString('N')+'.log')
Write-Host "Native packaged-character log: $log"
$runStarted = [DateTime]::UtcNow
# Fresh profiles share only immutable hash-addressed TAR data on the project
# drive, avoiding a gigabyte of system-drive extraction on each acceptance run.
$characterProduct = 'PackagedCharacterUnity-' + [Guid]::NewGuid().ToString('N')
$characterProfile = [IO.Path]::GetFullPath((Join-Path ([Environment]::GetFolderPath('LocalApplicationData')) ('../LocalLow/EclipseAcceptance/'+$characterProduct)))
$characterCache = Join-Path $characterProfile 'SF2DE/TarAssetCache'
$characterCacheTarget = Join-Path $fixture 'audio-tar-cache'
New-Item -ItemType Directory -Force -Path $characterCacheTarget | Out-Null
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $characterCache) | Out-Null
New-Item -ItemType Junction -Path $characterCache -Target $characterCacheTarget | Out-Null
$arguments=@('-projectPath',('"'+$fixture+'"'),'-executeMethod','PackagedCharacterUnity.Run','-packaged-characterAcceptanceProductName',$characterProduct,'-characterPackageModId',$characterModId,'-characterPackageModsRoot',('"'+$characterMods+'"'),'-logFile',('"'+$log+'"'))
if($OpponentOnly){$arguments+='-packagedOpponentOnly'}
$process = Start-Process -FilePath $Unity -ArgumentList $arguments -WindowStyle Hidden -PassThru
Write-Host "Native packaged-character process: $($process.Id)"
$deadline = [DateTime]::UtcNow.AddMinutes(15)
while (!$process.WaitForExit(20000)) {
    if ((Test-Path -LiteralPath $log) -and (Select-String -LiteralPath $log -Pattern 'error CS\d+:' -Quiet)) {$process.Kill();throw "Native packaged-character fixture failed compilation: $log"}
    if ([DateTime]::UtcNow -gt $deadline) { $process.Kill(); throw "Native packaged-character timed out: $log" }
}
Select-String -LiteralPath $log -Pattern '\[PackagedCharacterUnity\]|error CS' | ForEach-Object { Write-Host $_.Line }
$result = Join-Path $fixture 'packaged-character-result.txt'
if ($process.ExitCode -ne 0 -or !(Test-Path -LiteralPath $result) -or (Get-Item -LiteralPath $result).LastWriteTimeUtc -lt $runStarted) { throw "Native packaged-character acceptance failed: $log" }
Get-Content -LiteralPath $result
