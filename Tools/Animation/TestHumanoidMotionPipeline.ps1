param([Parameter(Mandatory=$true)][string]$Blender,[Parameter(Mandatory=$true)][string]$Rig)
$ErrorActionPreference='Stop'
$root=Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$rigPath=(Resolve-Path -LiteralPath $Rig).Path
$fixture=Join-Path $root ('Temp/HumanoidMotion-'+[Guid]::NewGuid().ToString('N'))
Write-Host "Humanoid motion fixture: $fixture"
& $Blender --background --factory-startup --python-exit-code 1 --python (Join-Path $PSScriptRoot 'CreateImportedRigFixture.py') -- --output $fixture
if($LASTEXITCODE -ne 0){throw 'Original rig creation failed.'}
& $Blender --background --factory-startup --python-exit-code 1 --python (Join-Path $PSScriptRoot 'CreateHumanoidMotionFixture.py') -- --source (Join-Path $fixture 'foreign.blend') --output (Join-Path $fixture 'motion.blend')
if($LASTEXITCODE -ne 0){throw 'Original action creation failed.'}
$sourceHash=(Get-FileHash -LiteralPath (Join-Path $fixture 'motion.blend')).Hash
& $Blender --background --factory-startup --python-exit-code 1 --python (Join-Path $PSScriptRoot 'ValidateHumanoidMotionState.py') -- --source (Join-Path $fixture 'motion.blend') --rig $rigPath
if($LASTEXITCODE -ne 0){throw 'Action state isolation failed.'}
$package=Join-Path $fixture 'local.imported-motion'
& $Blender --background --factory-startup --python-exit-code 1 --python (Join-Path $PSScriptRoot 'ImportCharacter.py') -- --source (Join-Path $fixture 'motion.blend') --rig $rigPath --mod-id local.imported-motion --output $package --clip strike OriginalStrike Punch --clip jump OriginalJump Kick
if($LASTEXITCODE -ne 0){throw 'Weighted rig/source action import failed.'}
if((Get-FileHash -LiteralPath (Join-Path $fixture 'motion.blend')).Hash -ne $sourceHash){throw 'Source file changed.'}
python (Join-Path $PSScriptRoot 'ValidateHumanoidMotion.py') $package
if($LASTEXITCODE -ne 0){throw 'Action geometry/duration contracts failed.'}
& (Join-Path $PSScriptRoot 'TestCharacterLua.ps1') -Package $package -Packaged -Playable -ImportedRig -ImportedMotion
foreach($rejection in @(
    @{Name='missing-action';Clips=@('--clip','bad','Missing','Punch')},
    @{Name='duplicate-key';Clips=@('--clip','strike','OriginalStrike','Punch','--clip','jump','OriginalJump','Punch')},
    @{Name='duplicate-name';Clips=@('--clip','strike','OriginalStrike','Punch','--clip','strike','OriginalJump','Kick')},
    @{Name='unknown-control';Clips=@('--clip','strike','OriginalStrike','Sprint')}
)){
    $rejectedOutput=Join-Path (Join-Path $fixture $rejection.Name) 'local.invalid'
    $rejectedArguments=@('--background','--factory-startup','--python-exit-code','1','--python',(Join-Path $PSScriptRoot 'ImportCharacter.py'),'--','--source',(Join-Path $fixture 'motion.blend'),'--rig',$rigPath,'--mod-id','local.invalid','--output',$rejectedOutput)+$rejection.Clips
    & $Blender @rejectedArguments
    if($LASTEXITCODE -eq 0 -or (Test-Path -LiteralPath $rejectedOutput)){throw "Invalid clip accepted or partially published: $($rejection.Name)"}
}
Write-Host 'PASS: evaluated humanoid source actions, core dimensions, jump height, source preservation and actual Lua controls/AI. Native gameplay is separate.'
